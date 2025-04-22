using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Services;

namespace NoSTORE
{
    public class Program
    {
        public static void NoCacheJS()
        {
            DateTime now = DateTime.UtcNow;
            long unix = ((DateTimeOffset)now).ToUnixTimeSeconds();
        }
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
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<FilterService>();

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute( 
                name: "catalog",
                pattern: "catalog/{slug}",
                defaults: new { controller = "Catalog", action = "Details" });

            app.Run();
        }
    }
}
