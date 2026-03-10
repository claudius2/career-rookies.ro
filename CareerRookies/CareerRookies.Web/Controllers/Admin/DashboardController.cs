using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    [Route("Dashboard")]
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            WorkshopCount = await _context.Workshops.CountAsync(),
            UpcomingWorkshopCount = await _context.Workshops.CountAsync(w => w.Date > DateTime.UtcNow),
            PendingArticles = await _context.Articles.CountAsync(a => a.Status == ArticleStatus.Pending),
            TotalRegistrations = await _context.WorkshopRegistrations.CountAsync(),
            TestimonialCount = await _context.Testimonials.CountAsync(),
            ResourceCount = await _context.CareerResources.CountAsync()
        };
        return View("~/Views/Admin/Dashboard/Index.cshtml", model);
    }
}
