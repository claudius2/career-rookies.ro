using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Resources")]
public class ResourceManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResourceManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var resources = await _context.CareerResources
            .OrderBy(r => r.Category)
            .ThenBy(r => r.SortOrder)
            .ToListAsync();
        return View("~/Views/Admin/Resource/Index.cshtml", resources);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Resource/Create.cshtml");
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Url,Category,SortOrder")] CareerResource model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Resource/Create.cshtml", model);

        _context.CareerResources.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Resursa a fost creată.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await _context.CareerResources.FindAsync(id);
        if (resource == null) return NotFound();
        return View("~/Views/Admin/Resource/Edit.cshtml", resource);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Url,Category,SortOrder")] CareerResource model)
    {
        if (id != model.Id) return NotFound();
        var resource = await _context.CareerResources.FindAsync(id);
        if (resource == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Resource/Edit.cshtml", model);

        resource.Name = model.Name;
        resource.Url = model.Url;
        resource.Category = model.Category;
        resource.SortOrder = model.SortOrder;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Resursa a fost actualizată.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _context.CareerResources.FindAsync(id);
        if (resource == null) return NotFound();
        _context.CareerResources.Remove(resource);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Resursa a fost ștearsă.";
        return RedirectToAction("Index");
    }
}
