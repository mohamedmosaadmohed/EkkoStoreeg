using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Utilities;

namespace EkkoSoreeg.Web.DataSeed
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Check if roles exist, if not, create them
            if (!await roleManager.RoleExistsAsync(SD.AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
            }
            if (!await roleManager.RoleExistsAsync(SD.EditorRole))
            {
                await roleManager.CreateAsync(new IdentityRole(SD.EditorRole));
            }
            if (!await roleManager.RoleExistsAsync(SD.CustomerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(SD.CustomerRole));
            }

            // Ensure the default admin account exists
            var adminUser = await userManager.FindByEmailAsync("Ekkostoreeg@gmail.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Name = "Ekkostoreeg",
                    UserName = "Ekkostoreeg@gmail.com",
                    Email = "Ekkostoreeg@gmail.com",
                    Address = "",
                    Region = "",
                    City= ""
                };
                await userManager.CreateAsync(adminUser, "Ekkostoreeg");
                await userManager.AddToRoleAsync(adminUser, SD.AdminRole);

                // Generate email confirmation token and confirm email
                var token = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                await userManager.ConfirmEmailAsync(adminUser, token);
            }
            else if (!await userManager.IsEmailConfirmedAsync(adminUser))
            {
                // Confirm the email if it is not already confirmed
                var token = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                await userManager.ConfirmEmailAsync(adminUser, token);
            }
        }
    }

}
