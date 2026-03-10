using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            UpcomingWorkshops = await _context.Workshops
                .Where(w => w.Date > DateTime.UtcNow)
                .OrderBy(w => w.Date)
                .Take(3)
                .ToListAsync(),
            Testimonials = await _context.Testimonials
                .OrderBy(t => t.SortOrder)
                .Take(10)
                .ToListAsync(),
            RecentArticles = await _context.Articles
                .Where(a => a.Status == ArticleStatus.Approved)
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToListAsync(),
            FeaturedResources = await _context.CareerResources
                .OrderBy(r => r.SortOrder)
                .Take(6)
                .ToListAsync()
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
