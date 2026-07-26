using Microsoft.AspNetCore.Identity;

namespace SmartTask.Web.Infrastructure.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "ProjectManager",
                "Member"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole<int>
                        {
                            Name = role
                        });
                }
            }
        }
    }
}