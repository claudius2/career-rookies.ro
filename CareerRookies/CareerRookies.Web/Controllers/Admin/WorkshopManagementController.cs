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
    private readonly IHtmlSanitizerService _htmlSanitizer;

    private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm", ".mov" };

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public WorkshopManagementController(
        IWorkshopService workshopService,
        IFileService fileService,
        IAuditService auditService,
        IHtmlSanitizerService htmlSanitizer)
    {
        _workshopService = workshopService;
        _fileService = fileService;
        _auditService = auditService;
        _htmlSanitizer = htmlSanitizer;
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
        var model = new WorkshopFormViewModel
        {
            Speakers = new List<SpeakerFormItem> { new() }
        };
        return View("~/Views/Admin/Workshop/Create.cshtml", model);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkshopFormViewModel model)
    {
        if (model.Speakers == null || model.Speakers.Count == 0)
        {
            ModelState.AddModelError("", "Trebuie sa adaugati cel putin un speaker.");
            return View("~/Views/Admin/Workshop/Create.cshtml", model);
        }

        // Remove validation errors for old single-speaker properties
        ModelState.Remove("SpeakerName");
        ModelState.Remove("SpeakerDescription");

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Workshop/Create.cshtml", model);

        var workshop = new Workshop
        {
            Title = model.Title,
            Description = _htmlSanitizer.Sanitize(model.Description),
            Date = model.Date,
            MaxCapacity = model.MaxCapacity
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

        await _workshopService.CreateAsync(workshop);

        // Create/link speakers
        var speakerIds = new List<int>();
        for (int i = 0; i < model.Speakers.Count; i++)
        {
            var speakerForm = model.Speakers[i];
            var speaker = new Speaker
            {
                Name = speakerForm.Name,
                Description = speakerForm.Description
            };

            if (speakerForm.Image != null)
            {
                var path = await _fileService.SaveImageAsync(speakerForm.Image, "speakers");
                if (path != null) speaker.ImagePath = path;
            }

            await _workshopService.CreateSpeakerAsync(speaker);
            speakerIds.Add(speaker.Id);
        }

        await _workshopService.SetWorkshopSpeakersAsync(workshop.Id, speakerIds);
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
            ExistingImagePath = workshop.ImagePath,
            Speakers = workshop.WorkshopSpeakers
                .OrderBy(ws => ws.SortOrder)
                .Select(ws => new SpeakerFormItem
                {
                    Id = ws.Speaker.Id,
                    Name = ws.Speaker.Name,
                    Description = ws.Speaker.Description,
                    ExistingImagePath = ws.Speaker.ImagePath
                })
                .ToList()
        };

        if (model.Speakers.Count == 0)
            model.Speakers.Add(new SpeakerFormItem());

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

        if (model.Speakers == null || model.Speakers.Count == 0)
        {
            ModelState.AddModelError("", "Trebuie sa adaugati cel putin un speaker.");
            model.ExistingImagePath = workshop.ImagePath;
            return View("~/Views/Admin/Workshop/Edit.cshtml", model);
        }

        ModelState.Remove("SpeakerName");
        ModelState.Remove("SpeakerDescription");

        if (!ModelState.IsValid)
        {
            model.ExistingImagePath = workshop.ImagePath;
            return View("~/Views/Admin/Workshop/Edit.cshtml", model);
        }

        workshop.Title = model.Title;
        workshop.Description = _htmlSanitizer.Sanitize(model.Description);
        workshop.Date = model.Date;
        workshop.MaxCapacity = model.MaxCapacity;

        if (model.Image != null)
        {
            var path = await _fileService.SaveImageAsync(model.Image, "workshops");
            if (path == null)
            {
                ModelState.AddModelError("Image", "Fisierul nu a putut fi salvat.");
                model.ExistingImagePath = workshop.ImagePath;
                return View("~/Views/Admin/Workshop/Edit.cshtml", model);
            }
            _fileService.DeleteFile(workshop.ImagePath);
            workshop.ImagePath = path;
        }

        // Update speakers
        var speakerIds = new List<int>();
        for (int i = 0; i < model.Speakers.Count; i++)
        {
            var speakerForm = model.Speakers[i];

            if (speakerForm.Id.HasValue && speakerForm.Id.Value > 0)
            {
                // Update existing speaker
                var existingSpeaker = workshop.WorkshopSpeakers
                    .FirstOrDefault(ws => ws.SpeakerId == speakerForm.Id.Value)?.Speaker;

                if (existingSpeaker != null)
                {
                    existingSpeaker.Name = speakerForm.Name;
                    existingSpeaker.Description = speakerForm.Description;

                    if (speakerForm.Image != null)
                    {
                        var path = await _fileService.SaveImageAsync(speakerForm.Image, "speakers");
                        if (path != null)
                        {
                            _fileService.DeleteFile(existingSpeaker.ImagePath);
                            existingSpeaker.ImagePath = path;
                        }
                    }

                    await _workshopService.UpdateSpeakerAsync(existingSpeaker);
                    speakerIds.Add(existingSpeaker.Id);
                    continue;
                }
            }

            // Create new speaker
            var speaker = new Speaker
            {
                Name = speakerForm.Name,
                Description = speakerForm.Description
            };

            if (speakerForm.Image != null)
            {
                var path = await _fileService.SaveImageAsync(speakerForm.Image, "speakers");
                if (path != null) speaker.ImagePath = path;
            }
            else if (!string.IsNullOrEmpty(speakerForm.ExistingImagePath))
            {
                speaker.ImagePath = speakerForm.ExistingImagePath;
            }

            await _workshopService.CreateSpeakerAsync(speaker);
            speakerIds.Add(speaker.Id);
        }

        await _workshopService.SetWorkshopSpeakersAsync(workshop.Id, speakerIds);
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
