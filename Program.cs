using BookShopTest.Data;
using Microsoft.EntityFrameworkCore;
using BookShopTest.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace BookShopTest
{
    public class Program
    {
        public static async Task Main(string[] args)

        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options => 
            options.UseSqlServer(builder.Configuration.GetConnectionString("BookPortal")));



            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            builder.Services.AddRazorPages();

            builder.Services.AddSession();



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

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Books}/{action=Index}/{id?}");



            app.MapRazorPages();


            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}
