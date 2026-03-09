using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;

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
        ViewBag.WorkshopCount = await _context.Workshops.CountAsync();
        ViewBag.UpcomingWorkshopCount = await _context.Workshops.CountAsync(w => w.Date > DateTime.Now);
        ViewBag.PendingArticles = await _context.Articles.CountAsync(a => !a.IsApproved);
        ViewBag.TotalRegistrations = await _context.WorkshopRegistrations.CountAsync();
        ViewBag.TestimonialCount = await _context.Testimonials.CountAsync();
        ViewBag.ResourceCount = await _context.CareerResources.CountAsync();
        return View("~/Views/Admin/Dashboard/Index.cshtml");
    }
}
