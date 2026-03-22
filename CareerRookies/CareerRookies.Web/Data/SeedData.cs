using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, config, logger);
        await SeedStudentClassesAsync(context);
        await SeedSiteSettingsAsync(context);
        await SeedMissingSiteSettingsAsync(context);
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

    private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, IConfiguration config, ILogger logger)
    {
        var adminEmail = config["AdminSettings:Email"] ?? "admin@career-rookies.ro";
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
                logger.LogInformation("Admin user '{Email}' created successfully.", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin user '{Email}': {Errors}", adminEmail, errors);
            }
        }
    }

    private static async Task SeedStudentClassesAsync(ApplicationDbContext context)
    {
        if (await context.StudentClasses.AnyAsync()) return;

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
        if (await context.SiteSettings.AnyAsync()) return;

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

    private static async Task SeedMissingSiteSettingsAsync(ApplicationDbContext context)
    {
        var existingKeys = await context.SiteSettings.Select(s => s.Key).ToListAsync();

        var newSettings = new List<SiteSetting>
        {
            new() { Key = "AboutProjectText", Value = "<p>Career Rookies este o inițiativă dedicată elevilor din România care își doresc să-și construiască un viitor profesional de succes.</p><p>Misiunea noastră este să oferim tinerilor acces la workshop-uri, resurse și mentorat din partea profesioniștilor din diverse domenii. Credem că fiecare elev merită să aibă acces la informații și oportunități care să-l ajute să ia cele mai bune decizii pentru cariera sa.</p><p>Prin evenimentele și resursele noastre, conectăm elevii cu experți din industrie, consilieri de carieră și programe educaționale care le deschid noi perspective.</p>" },
            new() { Key = "FooterAboutText", Value = "Ajutăm liceenii să descopere lumea profesională prin workshop-uri interactive, resurse educative și oportunități reale de dezvoltare." },
            new() { Key = "FooterContactEmail", Value = "contact@career-rookies.ro" },
            new() { Key = "FooterContactLocation", Value = "Sibiu, România" },
            new() { Key = "FooterInstagramUrl", Value = "https://www.instagram.com/career_rookies/" }
        };

        var toAdd = newSettings.Where(s => !existingKeys.Contains(s.Key)).ToList();
        if (toAdd.Any())
        {
            context.SiteSettings.AddRange(toAdd);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCareerResourcesAsync(ApplicationDbContext context)
    {
        if (await context.CareerResources.AnyAsync()) return;

        var resources = new List<CareerResource>
        {
            new() { Name = "RIUF - Târg de universități", Url = "https://www.riuf.ro", Category = ResourceCategory.UniversityFair, SortOrder = 1 },
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
