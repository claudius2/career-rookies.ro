using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers;

public class ResourceController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResourceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var resources = await _context.CareerResources
            .OrderBy(r => r.Category)
            .ThenBy(r => r.SortOrder)
            .ToListAsync();

        var grouped = resources.GroupBy(r => r.Category).ToDictionary(g => g.Key, g => g.ToList());
        return View(grouped);
    }
}
