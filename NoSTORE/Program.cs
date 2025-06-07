using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Hubs;
using NoSTORE.Middlewares;
using NoSTORE.Services;
using NoSTORE.Settings;
using Org.BouncyCastle.Pkix;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NoSTORE
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<ProductService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<FilterService>();
            services.AddScoped<UserService>();
            services.AddScoped<RoleService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<OrderService>();
            return services;
        }
        public static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<JwtService>();
            services.Configure<AuthSettings>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("JwtOptions"));
            return services;
        }
        public static IServiceCollection AddEmailServices(this IServiceCollection services)
        {
            services.Configure<SmtpSettings>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("SmtpSettings"));
            services.AddScoped<EmailService>();
            services.AddScoped<VerificationService>();
            return services;
        }
    }
    public class Program
    {
        public static async Task Main(string[] args)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<MongoDbContext>(sp =>
            {
                var connectionString = builder.Configuration.GetConnectionString("MongoDb");
                var databaseName = "NotOnlyStore";
                return new MongoDbContext(connectionString, databaseName);
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var jwt = builder.Configuration.GetSection("JwtOptions").Get<AuthSettings>();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("JWT Auth failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = "auth_token";
                    options.LoginPath = "/";
                    options.AccessDeniedPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });
            builder.Services.AddAuthorization();

            builder.Services
                .AddAuthServices()
                .AddAppServices()
                .AddEmailServices()
                .AddSignalR();

            builder.Services.AddScoped<GuestMiddleware>();


            //builder.WebHost.UseUrls("https://26.208.227.42:5000"); // ׀אסרוינ

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<UserHub>("/userHub");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "catalog",
                pattern: "catalog/{slug}",
                defaults: new { controller = "Catalog", action = "Details" });

            app.MapControllerRoute(
                name: "product",
                pattern: "product/{id}/{seo}",
                defaults: new { controller = "Product", action = "Details" });

            app.UseMiddleware<GuestMiddleware>();

            app.Run();
        }
    }
}
