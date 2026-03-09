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
    public async Task<IActionResult> Create(Workshop model, IFormFile? image, IFormFile? speakerImage)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Workshop/Create.cshtml", model);

        if (image != null)
            model.ImagePath = await SaveFileAsync(image, "workshops");

        if (speakerImage != null)
            model.SpeakerImagePath = await SaveFileAsync(speakerImage, "speakers");

        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;

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
    public async Task<IActionResult> Edit(int id, Workshop model, IFormFile? image, IFormFile? speakerImage)
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
        workshop.UpdatedAt = DateTime.Now;

        if (image != null)
            workshop.ImagePath = await SaveFileAsync(image, "workshops");

        if (speakerImage != null)
            workshop.SpeakerImagePath = await SaveFileAsync(speakerImage, "speakers");

        await _context.SaveChangesAsync();
        TempData["Success"] = "Workshop-ul a fost actualizat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null) return NotFound();

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
    public async Task<IActionResult> UploadMedia(int workshopId, IFormFile file, MediaType mediaType, string? youtubeUrl)
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
            media.FilePath = youtubeUrl ?? "";
        }
        else if (file != null)
        {
            media.FilePath = await SaveFileAsync(file, "workshop-media");
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
        _context.WorkshopMedia.Remove(media);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Media a fost ștearsă.";
        return RedirectToAction("Media", new { workshopId });
    }

    private async Task<string> SaveFileAsync(IFormFile file, string subfolder)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{subfolder}/{fileName}";
    }
}
