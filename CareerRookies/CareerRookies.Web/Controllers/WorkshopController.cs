using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

public class WorkshopController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkshopController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Upcoming()
    {
        var workshops = await _context.Workshops
            .Where(w => w.Date > DateTime.UtcNow)
            .OrderBy(w => w.Date)
            .ToListAsync();
        return View(workshops);
    }

    public async Task<IActionResult> Past()
    {
        var workshops = await _context.Workshops
            .Where(w => w.Date <= DateTime.UtcNow)
            .OrderByDescending(w => w.Date)
            .ToListAsync();
        return View(workshops);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workshop == null) return NotFound();

        var model = new WorkshopDetailViewModel
        {
            Workshop = workshop,
            Media = workshop.Media.ToList(),
            StudentClasses = await _context.StudentClasses
                .Where(sc => sc.IsActive)
                .ToListAsync(),
            Registration = new WorkshopRegistrationViewModel { WorkshopId = id }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(WorkshopRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await ReloadDetailView(model);
        }

        var workshop = await _context.Workshops.FindAsync(model.WorkshopId);
        if (workshop == null) return NotFound();

        // Prevent registration for past workshops
        if (workshop.Date <= DateTime.UtcNow)
        {
            TempData["Error"] = "Nu te poți înscrie la un workshop care a trecut.";
            return RedirectToAction("Detail", new { id = model.WorkshopId });
        }

        // Prevent duplicate registrations
        var alreadyRegistered = await _context.WorkshopRegistrations
            .AnyAsync(r => r.WorkshopId == model.WorkshopId
                        && r.StudentName == model.StudentName
                        && r.StudentClassId == model.StudentClassId);

        if (alreadyRegistered)
        {
            TempData["Error"] = "Ești deja înscris la acest workshop.";
            return RedirectToAction("Detail", new { id = model.WorkshopId });
        }

        var registration = new WorkshopRegistration
        {
            WorkshopId = model.WorkshopId,
            StudentName = model.StudentName,
            StudentClassId = model.StudentClassId,
            GdprConsentAccepted = model.GdprConsentAccepted,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkshopRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Înscrierea a fost realizată cu succes!";
        return RedirectToAction("Detail", new { id = model.WorkshopId });
    }

    private async Task<IActionResult> ReloadDetailView(WorkshopRegistrationViewModel model)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(w => w.Id == model.WorkshopId);

        if (workshop == null) return NotFound();

        var detailModel = new WorkshopDetailViewModel
        {
            Workshop = workshop,
            Media = workshop.Media.ToList(),
            StudentClasses = await _context.StudentClasses.Where(sc => sc.IsActive).ToListAsync(),
            Registration = model
        };
        return View("Detail", detailModel);
    }
}
