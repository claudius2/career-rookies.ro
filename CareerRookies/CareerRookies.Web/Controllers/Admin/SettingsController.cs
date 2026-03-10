using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Settings")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var settings = await _context.SiteSettings.OrderBy(s => s.Key).ToListAsync();
        return View("~/Views/Admin/Settings/Index.cshtml", settings);
    }

    [HttpPost]
    [Route("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Dictionary<string, string>? settings)
    {
        if (settings == null || settings.Count == 0)
        {
            TempData["Error"] = "Nu s-au primit date valide.";
            return RedirectToAction("Index");
        }

        var allSettings = await _context.SiteSettings.ToListAsync();
        var settingsDict = allSettings.ToDictionary(s => s.Key, s => s);

        foreach (var kvp in settings)
        {
            if (settingsDict.TryGetValue(kvp.Key, out var setting))
            {
                setting.Value = kvp.Value;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Setările au fost actualizate.";
        return RedirectToAction("Index");
    }
}
