using EnterpriseCollegeManagement.IdentityService.Configurations;
using EnterpriseCollegeManagement.IdentityService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EnterpriseCollegeManagement.IdentityService.Data
{
    public class DbInitializer
    {
        public static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "Teacher",
                "Student"
            };

            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

        }
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            IOptions<AdminSeedSettings> adminSettings)
        {
            var settings = adminSettings.Value;

            var existingAdmin = await userManager.FindByEmailAsync(settings.Email);

            if(existingAdmin != null)
            {
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = settings.Email,
                Email = settings.Email,
                FirstName = settings.FirstName,
                LastName = settings.LastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, settings.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(x => x.Description));

                throw new Exception(
                    $"Failed to create default admin user: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        


    }
}
