using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Middleware;
using CareerRookies.Web.Services;
using CareerRookies.Web.Services.Interfaces;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Identity
    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/autentificare";
        options.LogoutPath = "/deconectare";
        options.AccessDeniedPath = "/acces-interzis";
    });

    // Services
    builder.Services.AddScoped<IWorkshopService, WorkshopService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<ITestimonialService, TestimonialService>();
    builder.Services.AddScoped<IResourceService, ResourceService>();
    builder.Services.AddScoped<ISettingsService, SettingsService>();
    builder.Services.AddScoped<IStudentClassService, StudentClassService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
    builder.Services.AddSingleton<IRecaptchaService, RecaptchaService>();
    builder.Services.AddHttpClient();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (allowedOrigins is { Length: > 0 })
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
            else
            {
                // Default: same-origin only (no additional origins)
                policy.AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("PublicForm", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("General", opt =>
        {
            opt.PermitLimit = 60;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    // Response Caching
    builder.Services.AddResponseCaching();
    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5)));
        options.AddPolicy("Static", b => b.Expire(TimeSpan.FromHours(1)));
    });

    var app = builder.Build();

    // Seed data
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database.");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/eroare");
        app.UseHsts();
    }

    app.UseSecurityHeaders();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("Default");
    app.UseRateLimiter();
    app.UseResponseCaching();
    app.UseOutputCache();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging();

    app.MapHealthChecks("/health");

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
