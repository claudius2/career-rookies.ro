using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/StudentClasses")]
public class StudentClassManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentClassManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var classes = await _context.StudentClasses
            .OrderBy(sc => sc.Name)
            .ToListAsync();
        return View("~/Views/Admin/StudentClass/Index.cshtml", classes);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/StudentClass/Create.cshtml");
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,IsActive")] StudentClass model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/StudentClass/Create.cshtml", model);

        _context.StudentClasses.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Clasa a fost adăugată.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var sc = await _context.StudentClasses.FindAsync(id);
        if (sc == null) return NotFound();
        return View("~/Views/Admin/StudentClass/Edit.cshtml", sc);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] StudentClass model)
    {
        if (id != model.Id) return NotFound();
        var sc = await _context.StudentClasses.FindAsync(id);
        if (sc == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/StudentClass/Edit.cshtml", model);

        sc.Name = model.Name;
        sc.IsActive = model.IsActive;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Clasa a fost actualizată.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sc = await _context.StudentClasses.FindAsync(id);
        if (sc == null) return NotFound();
        _context.StudentClasses.Remove(sc);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Clasa a fost ștearsă.";
        return RedirectToAction("Index");
    }
}
