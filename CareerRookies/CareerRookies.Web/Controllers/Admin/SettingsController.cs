using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Settings")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAllAsync();
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

        await _settingsService.UpdateAsync(settings);
        TempData["Success"] = "Setarile au fost actualizate.";
        return RedirectToAction("Index");
    }
}
