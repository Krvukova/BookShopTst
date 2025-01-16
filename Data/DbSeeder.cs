using BookShopTest.Areas.Identity.Data;
using BookShopTest.Constants;
using Microsoft.AspNetCore.Identity;
using System;

namespace BookShopTest.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Seed Roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();  // Corrected to RoleManager<IdentityRole>

            // Ensure userManager and roleManager are not null
            if (userManager == null || roleManager == null)
            {
                throw new InvalidOperationException("UserManager or RoleManager is not registered in the DI container.");
            }

            // Create roles if they don't exist
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));

            // Create admin user
            var user = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FirstName = "Kristijan",
                LastName = "Vukovac",// Corrected property to match ApplicationUser class
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var userInDb = await userManager.FindByEmailAsync(user.Email);
            if (userInDb == null)
            {
                await userManager.CreateAsync(user, "Admin@123");
                await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }
        }
    }
}
