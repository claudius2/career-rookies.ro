using Microsoft.AspNetCore.Identity;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, config);
        await SeedStudentClassesAsync(context);
        await SeedSiteSettingsAsync(context);
        await SeedCareerResourcesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, IConfiguration config)
    {
        var adminEmail = config["AdminSettings:Email"] ?? "admin@careerrookies.ro";
        var adminPassword = config["AdminSettings:Password"] ?? "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    private static async Task SeedStudentClassesAsync(ApplicationDbContext context)
    {
        if (context.StudentClasses.Any()) return;

        var classes = new List<StudentClass>
        {
            new() { Name = "Clasa a IX-a", IsActive = true },
            new() { Name = "Clasa a X-a", IsActive = true },
            new() { Name = "Clasa a XI-a", IsActive = true },
            new() { Name = "Clasa a XII-a", IsActive = true },
            new() { Name = "Student universitar", IsActive = true },
            new() { Name = "Altele", IsActive = true }
        };

        context.StudentClasses.AddRange(classes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
    {
        if (context.SiteSettings.Any()) return;

        var settings = new List<SiteSetting>
        {
            new() { Key = "ArticlesSectionVisible", Value = "true" },
            new() { Key = "TestimonialsSectionVisible", Value = "true" },
            new() { Key = "ResourcesSectionVisible", Value = "true" },
            new() { Key = "WorkshopRegistrationEnabled", Value = "true" },
            new() { Key = "ArticleSubmissionEnabled", Value = "true" }
        };

        context.SiteSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCareerResourcesAsync(ApplicationDbContext context)
    {
        if (context.CareerResources.Any()) return;

        var resources = new List<CareerResource>
        {
            new() { Name = "RIUF - Pair de universități", Url = "https://www.riuf.ro", Category = ResourceCategory.UniversityFair, SortOrder = 1 },
            new() { Name = "EducationUSA", Url = "https://educationusa.state.gov", Category = ResourceCategory.UniversityFair, SortOrder = 2 },
            new() { Name = "Cognitrom", Url = "https://www.cognitrom.ro", Category = ResourceCategory.GuidanceFirm, SortOrder = 1 },
            new() { Name = "Smartree", Url = "https://www.smartree.com", Category = ResourceCategory.GuidanceFirm, SortOrder = 2 },
            new() { Name = "Erasmus+", Url = "https://erasmus-plus.ec.europa.eu", Category = ResourceCategory.EUProject, SortOrder = 1 },
            new() { Name = "Euroguidance", Url = "https://www.euroguidance.eu", Category = ResourceCategory.EUProject, SortOrder = 2 }
        };

        context.CareerResources.AddRange(resources);
        await context.SaveChangesAsync();
    }
}
