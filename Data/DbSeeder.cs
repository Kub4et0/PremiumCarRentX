using Microsoft.AspNetCore.Identity;
using Rent_a_car.Models;

namespace Rent_a_car.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Client", "Driver" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            var adminEmail = "admin@rentacar.com";

            var adminUser = await userManager
                .FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    FullName = "System Administrator",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager
                    .CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager
                        .AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
