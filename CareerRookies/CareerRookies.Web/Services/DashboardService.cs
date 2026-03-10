using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        // Single query to get all counts at once
        var now = DateTime.UtcNow;

        var stats = await _context.Workshops
            .Where(w => !w.IsDeleted)
            .GroupBy(w => 1)
            .Select(g => new
            {
                WorkshopCount = g.Count(),
                UpcomingWorkshopCount = g.Count(w => w.Date > now)
            })
            .FirstOrDefaultAsync();

        var pendingArticles = await _context.Articles
            .CountAsync(a => a.Status == ArticleStatus.Pending && !a.IsDeleted);

        var totalRegistrations = await _context.WorkshopRegistrations.CountAsync();

        var testimonialCount = await _context.Testimonials
            .CountAsync(t => !t.IsDeleted);

        var resourceCount = await _context.CareerResources.CountAsync();

        return new DashboardViewModel
        {
            WorkshopCount = stats?.WorkshopCount ?? 0,
            UpcomingWorkshopCount = stats?.UpcomingWorkshopCount ?? 0,
            PendingArticles = pendingArticles,
            TotalRegistrations = totalRegistrations,
            TestimonialCount = testimonialCount,
            ResourceCount = resourceCount
        };
    }
}
