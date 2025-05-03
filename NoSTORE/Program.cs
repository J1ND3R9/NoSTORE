using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Services;
using NoSTORE.Settings;
using Org.BouncyCastle.Pkix;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<MongoDbContext>(sp =>
            {
                var connectionString = builder.Configuration.GetConnectionString("MongoDb");
                var databaseName = "NotOnlyStore";
                return new MongoDbContext(connectionString, databaseName);
            });
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
            ValidAudience = builder.Configuration["JwtOptions:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JwtOptions:SecretKey"])),
        };
    });

            builder.Services.AddAuthorization();

            builder.Services
                .AddAppServices()
                .AddAuthServices()
                .AddEmailServices();

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

            app.UseAuthorization();
            app.UseAuthentication();

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

            app.Run();
        }
    }
}
