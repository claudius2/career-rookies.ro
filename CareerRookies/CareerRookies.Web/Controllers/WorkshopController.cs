using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

public class WorkshopController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly IStudentClassService _studentClassService;

    public WorkshopController(IWorkshopService workshopService, IStudentClassService studentClassService)
    {
        _workshopService = workshopService;
        _studentClassService = studentClassService;
    }

    public async Task<IActionResult> Upcoming(int page = 1)
    {
        var workshops = await _workshopService.GetUpcomingAsync(page);
        return View(workshops);
    }

    public async Task<IActionResult> Past(int page = 1)
    {
        var workshops = await _workshopService.GetPastAsync(page);
        return View(workshops);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var workshop = await _workshopService.GetWithMediaAsync(id);
        if (workshop == null) return NotFound();

        var registrationCount = await _workshopService.GetRegistrationCountAsync(id);

        var model = new WorkshopDetailViewModel
        {
            Workshop = workshop,
            Media = workshop.Media.ToList(),
            StudentClasses = await _studentClassService.GetActiveAsync(),
            Registration = new WorkshopRegistrationViewModel { WorkshopId = id },
            RegistrationCount = registrationCount
        };

        return View(model);
    }

    [Route("Workshop/{slug}")]
    public async Task<IActionResult> DetailBySlug(string slug)
    {
        var workshop = await _workshopService.GetBySlugAsync(slug);
        if (workshop == null) return NotFound();

        var registrationCount = await _workshopService.GetRegistrationCountAsync(workshop.Id);

        var model = new WorkshopDetailViewModel
        {
            Workshop = workshop,
            Media = workshop.Media.ToList(),
            StudentClasses = await _studentClassService.GetActiveAsync(),
            Registration = new WorkshopRegistrationViewModel { WorkshopId = workshop.Id },
            RegistrationCount = registrationCount
        };

        return View("Detail", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(WorkshopRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await ReloadDetailView(model);
        }

        var workshop = await _workshopService.GetByIdAsync(model.WorkshopId);
        if (workshop == null) return NotFound();

        if (workshop.Date <= DateTime.UtcNow)
        {
            TempData["Error"] = "Nu te poti inscrie la un workshop care a trecut.";
            return RedirectToAction("Detail", new { id = model.WorkshopId });
        }

        var canRegister = await _workshopService.CanRegisterAsync(model.WorkshopId, model.StudentName, model.StudentClassId);
        if (!canRegister)
        {
            TempData["Error"] = "Nu te mai poti inscrie la acest workshop (esti deja inscris sau workshop-ul este plin).";
            return RedirectToAction("Detail", new { id = model.WorkshopId });
        }

        var registration = new WorkshopRegistration
        {
            WorkshopId = model.WorkshopId,
            StudentName = model.StudentName,
            StudentClassId = model.StudentClassId,
            GdprConsentAccepted = model.GdprConsentAccepted
        };

        await _workshopService.RegisterAsync(registration);

        TempData["Success"] = "Inscrierea a fost realizata cu succes!";
        return RedirectToAction("Detail", new { id = model.WorkshopId });
    }

    private async Task<IActionResult> ReloadDetailView(WorkshopRegistrationViewModel model)
    {
        var workshop = await _workshopService.GetWithMediaAsync(model.WorkshopId);
        if (workshop == null) return NotFound();

        var registrationCount = await _workshopService.GetRegistrationCountAsync(model.WorkshopId);

        var detailModel = new WorkshopDetailViewModel
        {
            Workshop = workshop,
            Media = workshop.Media.ToList(),
            StudentClasses = await _studentClassService.GetActiveAsync(),
            Registration = model,
            RegistrationCount = registrationCount
        };
        return View("Detail", detailModel);
    }
}
