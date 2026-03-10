using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

[Route("workshopuri")]
public class WorkshopController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly IStudentClassService _studentClassService;
    private readonly IRecaptchaService _recaptchaService;

    public WorkshopController(IWorkshopService workshopService, IStudentClassService studentClassService, IRecaptchaService recaptchaService)
    {
        _workshopService = workshopService;
        _studentClassService = studentClassService;
        _recaptchaService = recaptchaService;
    }

    [Route("viitoare")]
    public async Task<IActionResult> Upcoming(int page = 1)
    {
        var workshops = await _workshopService.GetUpcomingAsync(page);
        return View(workshops);
    }

    [Route("trecute")]
    public async Task<IActionResult> Past(int page = 1)
    {
        var workshops = await _workshopService.GetPastAsync(page);
        return View(workshops);
    }

    [Route("detalii/{id:int}")]
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

        SetRecaptchaViewBag();
        return View(model);
    }

    [Route("{slug}")]
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

        SetRecaptchaViewBag();
        return View("Detail", model);
    }

    [HttpPost]
    [Route("inscriere")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(WorkshopRegistrationViewModel model)
    {
        // Verify reCAPTCHA
        var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptchaService.VerifyAsync(recaptchaResponse))
        {
            ModelState.AddModelError(string.Empty, "Verificarea reCAPTCHA a eșuat. Încearcă din nou.");
        }

        if (!ModelState.IsValid)
        {
            return await ReloadDetailView(model);
        }

        var workshop = await _workshopService.GetByIdAsync(model.WorkshopId);
        if (workshop == null) return NotFound();

        if (workshop.Date <= DateTime.UtcNow)
        {
            TempData["Error"] = "Nu te poți înscrie la un workshop care a trecut.";
            return RedirectToAction("Detail", new { id = model.WorkshopId });
        }

        var canRegister = await _workshopService.CanRegisterAsync(model.WorkshopId, model.StudentName, model.StudentClassId);
        if (!canRegister)
        {
            TempData["Error"] = "Nu te mai poți înscrie la acest workshop (ești deja înscris sau workshop-ul este plin).";
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

        TempData["Success"] = "Înscrierea a fost realizată cu succes!";
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
        SetRecaptchaViewBag();
        return View("Detail", detailModel);
    }

    private void SetRecaptchaViewBag()
    {
        ViewBag.RecaptchaSiteKey = _recaptchaService.SiteKey;
        ViewBag.RecaptchaEnabled = _recaptchaService.IsEnabled;
    }
}
