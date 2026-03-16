using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels.Admin;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/TeamMembers")]
public class TeamMemberManagementController : Controller
{
    private readonly ITeamMemberService _teamMemberService;
    private readonly IFileService _fileService;
    private readonly IAuditService _auditService;

    public TeamMemberManagementController(
        ITeamMemberService teamMemberService,
        IFileService fileService,
        IAuditService auditService)
    {
        _teamMemberService = teamMemberService;
        _fileService = fileService;
        _auditService = auditService;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var members = await _teamMemberService.GetAllAsync();
        return View("~/Views/Admin/TeamMember/Index.cshtml", members);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/TeamMember/Create.cshtml", new TeamMemberFormViewModel());
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeamMemberFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/TeamMember/Create.cshtml", model);

        var member = new TeamMember
        {
            Name = model.Name,
            Description = model.Description,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        };

        if (model.Image != null)
        {
            member.ImagePath = await _fileService.SaveFileAsync(model.Image, "team", new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" });
        }

        await _teamMemberService.CreateAsync(member);
        await _auditService.LogAsync("TeamMember", member.Id, "Created", User.Identity?.Name);
        TempData["Success"] = "Membrul echipei a fost creat.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var member = await _teamMemberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var model = new TeamMemberFormViewModel
        {
            Id = member.Id,
            Name = member.Name,
            Description = member.Description,
            ExistingImagePath = member.ImagePath,
            SortOrder = member.SortOrder,
            IsActive = member.IsActive
        };

        return View("~/Views/Admin/TeamMember/Edit.cshtml", model);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TeamMemberFormViewModel model)
    {
        if (id != model.Id) return NotFound();
        var member = await _teamMemberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/TeamMember/Edit.cshtml", model);

        member.Name = model.Name;
        member.Description = model.Description;
        member.SortOrder = model.SortOrder;
        member.IsActive = model.IsActive;

        if (model.Image != null)
        {
            member.ImagePath = await _fileService.SaveFileAsync(model.Image, "team", new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" });
        }

        await _teamMemberService.UpdateAsync(member);
        await _auditService.LogAsync("TeamMember", member.Id, "Updated", User.Identity?.Name);
        TempData["Success"] = "Membrul echipei a fost actualizat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _teamMemberService.DeleteAsync(id);
        await _auditService.LogAsync("TeamMember", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Membrul echipei a fost sters.";
        return RedirectToAction("Index");
    }
}
