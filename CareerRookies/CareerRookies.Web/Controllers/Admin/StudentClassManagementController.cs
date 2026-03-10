using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels.Admin;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/StudentClasses")]
public class StudentClassManagementController : Controller
{
    private readonly IStudentClassService _studentClassService;
    private readonly IAuditService _auditService;

    public StudentClassManagementController(IStudentClassService studentClassService, IAuditService auditService)
    {
        _studentClassService = studentClassService;
        _auditService = auditService;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var classes = await _studentClassService.GetAllAsync();
        return View("~/Views/Admin/StudentClass/Index.cshtml", classes);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/StudentClass/Create.cshtml", new StudentClassFormViewModel());
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentClassFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/StudentClass/Create.cshtml", model);

        var sc = new StudentClass
        {
            Name = model.Name,
            IsActive = model.IsActive
        };

        await _studentClassService.CreateAsync(sc);
        await _auditService.LogAsync("StudentClass", sc.Id, "Created", User.Identity?.Name);
        TempData["Success"] = "Clasa a fost adaugata.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var sc = await _studentClassService.GetByIdAsync(id);
        if (sc == null) return NotFound();

        var model = new StudentClassFormViewModel
        {
            Id = sc.Id,
            Name = sc.Name,
            IsActive = sc.IsActive
        };

        return View("~/Views/Admin/StudentClass/Edit.cshtml", model);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentClassFormViewModel model)
    {
        if (id != model.Id) return NotFound();
        var sc = await _studentClassService.GetByIdAsync(id);
        if (sc == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/StudentClass/Edit.cshtml", model);

        sc.Name = model.Name;
        sc.IsActive = model.IsActive;
        await _studentClassService.UpdateAsync(sc);
        await _auditService.LogAsync("StudentClass", sc.Id, "Updated", User.Identity?.Name);
        TempData["Success"] = "Clasa a fost actualizata.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _studentClassService.DeleteAsync(id);
        await _auditService.LogAsync("StudentClass", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Clasa a fost stearsa.";
        return RedirectToAction("Index");
    }
}
