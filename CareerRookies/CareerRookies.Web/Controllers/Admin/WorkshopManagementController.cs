using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Workshops")]
public class WorkshopManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm", ".mov" };

    private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

    public WorkshopManagementController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var workshops = await _context.Workshops
            .OrderByDescending(w => w.Date)
            .ToListAsync();
        return View("~/Views/Admin/Workshop/Index.cshtml", workshops);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Workshop/Create.cshtml");
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Date,SpeakerName,SpeakerDescription")] Workshop model,
        IFormFile? image, IFormFile? speakerImage)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Workshop/Create.cshtml", model);

        if (image != null)
        {
            var path = await SaveImageAsync(image, "workshops");
            if (path == null) return View("~/Views/Admin/Workshop/Create.cshtml", model);
            model.ImagePath = path;
        }

        if (speakerImage != null)
        {
            var path = await SaveImageAsync(speakerImage, "speakers");
            if (path == null) return View("~/Views/Admin/Workshop/Create.cshtml", model);
            model.SpeakerImagePath = path;
        }

        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;

        _context.Workshops.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Workshop-ul a fost creat cu succes.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null) return NotFound();
        return View("~/Views/Admin/Workshop/Edit.cshtml", workshop);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Title,Description,Date,SpeakerName,SpeakerDescription")] Workshop model,
        IFormFile? image, IFormFile? speakerImage)
    {
        if (id != model.Id) return NotFound();

        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Workshop/Edit.cshtml", model);

        workshop.Title = model.Title;
        workshop.Description = model.Description;
        workshop.Date = model.Date;
        workshop.SpeakerName = model.SpeakerName;
        workshop.SpeakerDescription = model.SpeakerDescription;

        if (image != null)
        {
            var path = await SaveImageAsync(image, "workshops");
            if (path == null) return View("~/Views/Admin/Workshop/Edit.cshtml", model);
            DeletePhysicalFile(workshop.ImagePath);
            workshop.ImagePath = path;
        }

        if (speakerImage != null)
        {
            var path = await SaveImageAsync(speakerImage, "speakers");
            if (path == null) return View("~/Views/Admin/Workshop/Edit.cshtml", model);
            DeletePhysicalFile(workshop.SpeakerImagePath);
            workshop.SpeakerImagePath = path;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Workshop-ul a fost actualizat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Media)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (workshop == null) return NotFound();

        // Clean up physical files
        DeletePhysicalFile(workshop.ImagePath);
        DeletePhysicalFile(workshop.SpeakerImagePath);
        foreach (var media in workshop.Media)
        {
            if (media.MediaType != MediaType.YouTubeLink)
                DeletePhysicalFile(media.FilePath);
        }

        _context.Workshops.Remove(workshop);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Workshop-ul a fost șters.";
        return RedirectToAction("Index");
    }

    [Route("Registrations/{workshopId}")]
    public async Task<IActionResult> Registrations(int workshopId)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Registrations)
                .ThenInclude(r => r.StudentClass)
            .FirstOrDefaultAsync(w => w.Id == workshopId);
        if (workshop == null) return NotFound();
        return View("~/Views/Admin/Workshop/Registrations.cshtml", workshop);
    }

    [Route("Media/{workshopId}")]
    public async Task<IActionResult> Media(int workshopId)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(w => w.Id == workshopId);
        if (workshop == null) return NotFound();
        return View("~/Views/Admin/Workshop/Media.cshtml", workshop);
    }

    [HttpPost]
    [Route("Media/{workshopId}/Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadMedia(int workshopId, IFormFile? file, MediaType mediaType, string? youtubeUrl)
    {
        var workshop = await _context.Workshops.FindAsync(workshopId);
        if (workshop == null) return NotFound();

        var media = new WorkshopMedia
        {
            WorkshopId = workshopId,
            MediaType = mediaType,
            SortOrder = await _context.WorkshopMedia.CountAsync(m => m.WorkshopId == workshopId) + 1
        };

        if (mediaType == MediaType.YouTubeLink)
        {
            if (string.IsNullOrWhiteSpace(youtubeUrl) || !IsValidYouTubeUrl(youtubeUrl))
            {
                TempData["Error"] = "Introduceți un URL YouTube valid.";
                return RedirectToAction("Media", new { workshopId });
            }
            media.FilePath = youtubeUrl;
        }
        else if (file != null)
        {
            var allowedExtensions = mediaType == MediaType.Video ? AllowedVideoExtensions : AllowedImageExtensions;
            var path = await SaveFileAsync(file, "workshop-media", allowedExtensions);
            if (path == null)
            {
                return RedirectToAction("Media", new { workshopId });
            }
            media.FilePath = path;
        }
        else
        {
            TempData["Error"] = "Selectați un fișier.";
            return RedirectToAction("Media", new { workshopId });
        }

        _context.WorkshopMedia.Add(media);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Media a fost adăugată.";
        return RedirectToAction("Media", new { workshopId });
    }

    [HttpPost]
    [Route("Media/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        var media = await _context.WorkshopMedia.FindAsync(id);
        if (media == null) return NotFound();
        var workshopId = media.WorkshopId;

        // Clean up physical file
        if (media.MediaType != MediaType.YouTubeLink)
            DeletePhysicalFile(media.FilePath);

        _context.WorkshopMedia.Remove(media);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Media a fost ștearsă.";
        return RedirectToAction("Media", new { workshopId });
    }

    private async Task<string?> SaveImageAsync(IFormFile file, string subfolder)
    {
        return await SaveFileAsync(file, subfolder, AllowedImageExtensions);
    }

    private async Task<string?> SaveFileAsync(IFormFile file, string subfolder, HashSet<string> allowedExtensions)
    {
        if (file.Length > MaxFileSize)
        {
            TempData["Error"] = $"Fișierul este prea mare. Dimensiunea maximă este {MaxFileSize / (1024 * 1024)} MB.";
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            TempData["Error"] = $"Tipul de fișier '{extension}' nu este permis. Extensii permise: {string.Join(", ", allowedExtensions)}";
            return null;
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{subfolder}/{fileName}";
    }

    private void DeletePhysicalFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private static bool IsValidYouTubeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        var host = uri.Host.ToLowerInvariant();
        return host.Contains("youtube.com") || host.Contains("youtu.be");
    }
}
