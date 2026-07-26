using Microsoft.AspNetCore.Identity;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Infrastructure.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@smarttask.com";
            const string adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin != null)
                return;

            admin = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Administrator",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true,
                TimeZone = "Asia/Tehran"
            };

            var result = await userManager.CreateAsync(
                admin,
                adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");
            }
        }
    }
}