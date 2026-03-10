using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Controllers;

public class ResourceController : Controller
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<IActionResult> Index()
    {
        var grouped = await _resourceService.GetGroupedAsync();
        return View(grouped);
    }
}
