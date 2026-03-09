using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;

namespace CareerRookies.Web.Controllers;

public class TestimonialController : Controller
{
    private readonly ApplicationDbContext _context;

    public TestimonialController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var testimonials = await _context.Testimonials
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
        return View(testimonials);
    }
}
