using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels.Admin;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Workshops")]
public class WorkshopManagementController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly IFileService _fileService;
    private readonly IAuditService _auditService;

    private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm", ".mov" };

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public WorkshopManagementController(
        IWorkshopService workshopService,
        IFileService fileService,
        IAuditService auditService)
    {
        _workshopService = workshopService;
        _fileService = fileService;
        _auditService = auditService;
    }

    [Route("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var workshops = await _workshopService.GetAllAsync(page);
        return View("~/Views/Admin/Workshop/Index.cshtml", workshops);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Workshop/Create.cshtml", new WorkshopFormViewModel());
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkshopFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Workshop/Create.cshtml", model);

        var workshop = new Workshop
        {
            Title = model.Title,
            Description = model.Description,
            Date = model.Date,
            MaxCapacity = model.MaxCapacity,
            SpeakerName = model.SpeakerName,
            SpeakerDescription = model.SpeakerDescription
        };

        if (model.Image != null)
        {
            var path = await _fileService.SaveImageAsync(model.Image, "workshops");
            if (path == null)
            {
                ModelState.AddModelError("Image", "Fisierul nu a putut fi salvat. Verificati tipul si dimensiunea.");
                return View("~/Views/Admin/Workshop/Create.cshtml", model);
            }
            workshop.ImagePath = path;
        }

        if (model.SpeakerImage != null)
        {
            var path = await _fileService.SaveImageAsync(model.SpeakerImage, "speakers");
            if (path == null)
            {
                ModelState.AddModelError("SpeakerImage", "Fisierul nu a putut fi salvat. Verificati tipul si dimensiunea.");
                return View("~/Views/Admin/Workshop/Create.cshtml", model);
            }
            workshop.SpeakerImagePath = path;
        }

        await _workshopService.CreateAsync(workshop);
        await _auditService.LogAsync("Workshop", workshop.Id, "Created", User.Identity?.Name);

        TempData["Success"] = "Workshop-ul a fost creat cu succes.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var workshop = await _workshopService.GetByIdAsync(id);
        if (workshop == null) return NotFound();

        var model = new WorkshopFormViewModel
        {
            Id = workshop.Id,
            Title = workshop.Title,
            Description = workshop.Description,
            Date = workshop.Date,
            MaxCapacity = workshop.MaxCapacity,
            SpeakerName = workshop.SpeakerName,
            SpeakerDescription = workshop.SpeakerDescription,
            ExistingImagePath = workshop.ImagePath,
            ExistingSpeakerImagePath = workshop.SpeakerImagePath
        };

        return View("~/Views/Admin/Workshop/Edit.cshtml", model);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkshopFormViewModel model)
    {
        if (id != model.Id) return NotFound();

        var workshop = await _workshopService.GetByIdAsync(id);
        if (workshop == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.ExistingImagePath = workshop.ImagePath;
            model.ExistingSpeakerImagePath = workshop.SpeakerImagePath;
            return View("~/Views/Admin/Workshop/Edit.cshtml", model);
        }

        workshop.Title = model.Title;
        workshop.Description = model.Description;
        workshop.Date = model.Date;
        workshop.MaxCapacity = model.MaxCapacity;
        workshop.SpeakerName = model.SpeakerName;
        workshop.SpeakerDescription = model.SpeakerDescription;

        if (model.Image != null)
        {
            var path = await _fileService.SaveImageAsync(model.Image, "workshops");
            if (path == null)
            {
                ModelState.AddModelError("Image", "Fisierul nu a putut fi salvat.");
                model.ExistingImagePath = workshop.ImagePath;
                model.ExistingSpeakerImagePath = workshop.SpeakerImagePath;
                return View("~/Views/Admin/Workshop/Edit.cshtml", model);
            }
            _fileService.DeleteFile(workshop.ImagePath);
            workshop.ImagePath = path;
        }

        if (model.SpeakerImage != null)
        {
            var path = await _fileService.SaveImageAsync(model.SpeakerImage, "speakers");
            if (path == null)
            {
                ModelState.AddModelError("SpeakerImage", "Fisierul nu a putut fi salvat.");
                model.ExistingImagePath = workshop.ImagePath;
                model.ExistingSpeakerImagePath = workshop.SpeakerImagePath;
                return View("~/Views/Admin/Workshop/Edit.cshtml", model);
            }
            _fileService.DeleteFile(workshop.SpeakerImagePath);
            workshop.SpeakerImagePath = path;
        }

        await _workshopService.UpdateAsync(workshop);
        await _auditService.LogAsync("Workshop", workshop.Id, "Updated", User.Identity?.Name);

        TempData["Success"] = "Workshop-ul a fost actualizat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _workshopService.SoftDeleteAsync(id);
        await _auditService.LogAsync("Workshop", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Workshop-ul a fost sters.";
        return RedirectToAction("Index");
    }

    [Route("Registrations/{workshopId}")]
    public async Task<IActionResult> Registrations(int workshopId)
    {
        var workshop = await _workshopService.GetWithRegistrationsAsync(workshopId);
        if (workshop == null) return NotFound();
        return View("~/Views/Admin/Workshop/Registrations.cshtml", workshop);
    }

    [Route("Registrations/{workshopId}/Export")]
    public async Task<IActionResult> ExportRegistrations(int workshopId)
    {
        var csvBytes = await _workshopService.ExportRegistrationsCsvAsync(workshopId);
        return File(csvBytes, "text/csv", $"registrations-workshop-{workshopId}.csv");
    }

    [Route("Media/{workshopId}")]
    public async Task<IActionResult> Media(int workshopId)
    {
        var workshop = await _workshopService.GetWithMediaAsync(workshopId);
        if (workshop == null) return NotFound();
        return View("~/Views/Admin/Workshop/Media.cshtml", workshop);
    }

    [HttpPost]
    [Route("Media/{workshopId}/Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadMedia(int workshopId, IFormFile? file, MediaType mediaType, string? youtubeUrl)
    {
        var workshop = await _workshopService.GetByIdAsync(workshopId);
        if (workshop == null) return NotFound();

        var media = new WorkshopMedia
        {
            WorkshopId = workshopId,
            MediaType = mediaType,
            SortOrder = await _workshopService.GetNextMediaSortOrderAsync(workshopId)
        };

        if (mediaType == MediaType.YouTubeLink)
        {
            if (string.IsNullOrWhiteSpace(youtubeUrl) || !_fileService.IsValidYouTubeUrl(youtubeUrl))
            {
                TempData["Error"] = "Introduceti un URL YouTube valid.";
                return RedirectToAction("Media", new { workshopId });
            }
            media.FilePath = youtubeUrl;
        }
        else if (file != null)
        {
            var allowedExtensions = mediaType == MediaType.Video ? AllowedVideoExtensions : AllowedImageExtensions;
            var path = await _fileService.SaveFileAsync(file, "workshop-media", allowedExtensions);
            if (path == null)
            {
                TempData["Error"] = "Fisierul nu a putut fi salvat. Verificati tipul si dimensiunea.";
                return RedirectToAction("Media", new { workshopId });
            }
            media.FilePath = path;
        }
        else
        {
            TempData["Error"] = "Selectati un fisier.";
            return RedirectToAction("Media", new { workshopId });
        }

        await _workshopService.AddMediaAsync(media);
        await _auditService.LogAsync("WorkshopMedia", media.Id, "Created", User.Identity?.Name);
        TempData["Success"] = "Media a fost adaugata.";
        return RedirectToAction("Media", new { workshopId });
    }

    [HttpPost]
    [Route("Media/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        var media = await _workshopService.GetMediaByIdAsync(id);
        if (media == null) return NotFound();
        var workshopId = media.WorkshopId;

        if (media.MediaType != MediaType.YouTubeLink)
            _fileService.DeleteFile(media.FilePath);

        await _workshopService.DeleteMediaAsync(id);
        await _auditService.LogAsync("WorkshopMedia", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Media a fost stearsa.";
        return RedirectToAction("Media", new { workshopId });
    }
}
