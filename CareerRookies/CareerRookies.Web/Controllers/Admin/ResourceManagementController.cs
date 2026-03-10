using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels.Admin;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Resources")]
public class ResourceManagementController : Controller
{
    private readonly IResourceService _resourceService;
    private readonly IAuditService _auditService;

    public ResourceManagementController(IResourceService resourceService, IAuditService auditService)
    {
        _resourceService = resourceService;
        _auditService = auditService;
    }

    [Route("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var resources = await _resourceService.GetAllAsync(page);
        return View("~/Views/Admin/Resource/Index.cshtml", resources);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Resource/Create.cshtml", new ResourceFormViewModel());
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Resource/Create.cshtml", model);

        var resource = new CareerResource
        {
            Name = model.Name,
            Url = model.Url,
            Category = model.Category,
            SortOrder = model.SortOrder
        };

        await _resourceService.CreateAsync(resource);
        await _auditService.LogAsync("CareerResource", resource.Id, "Created", User.Identity?.Name);
        TempData["Success"] = "Resursa a fost creata.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await _resourceService.GetByIdAsync(id);
        if (resource == null) return NotFound();

        var model = new ResourceFormViewModel
        {
            Id = resource.Id,
            Name = resource.Name,
            Url = resource.Url,
            Category = resource.Category,
            SortOrder = resource.SortOrder
        };

        return View("~/Views/Admin/Resource/Edit.cshtml", model);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResourceFormViewModel model)
    {
        if (id != model.Id) return NotFound();
        var resource = await _resourceService.GetByIdAsync(id);
        if (resource == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Resource/Edit.cshtml", model);

        resource.Name = model.Name;
        resource.Url = model.Url;
        resource.Category = model.Category;
        resource.SortOrder = model.SortOrder;
        await _resourceService.UpdateAsync(resource);
        await _auditService.LogAsync("CareerResource", resource.Id, "Updated", User.Identity?.Name);
        TempData["Success"] = "Resursa a fost actualizata.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _resourceService.DeleteAsync(id);
        await _auditService.LogAsync("CareerResource", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Resursa a fost stearsa.";
        return RedirectToAction("Index");
    }
}
