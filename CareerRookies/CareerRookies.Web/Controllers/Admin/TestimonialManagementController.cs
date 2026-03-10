using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Testimonials")]
public class TestimonialManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public TestimonialManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var testimonials = await _context.Testimonials
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
        return View("~/Views/Admin/Testimonial/Index.cshtml", testimonials);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Testimonial/Create.cshtml");
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Text,AuthorName,AuthorType,SortOrder")] Testimonial model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Testimonial/Create.cshtml", model);

        model.CreatedAt = DateTime.UtcNow;
        _context.Testimonials.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Testimonialul a fost creat.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return NotFound();
        return View("~/Views/Admin/Testimonial/Edit.cshtml", testimonial);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Text,AuthorName,AuthorType,SortOrder")] Testimonial model)
    {
        if (id != model.Id) return NotFound();
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Testimonial/Edit.cshtml", model);

        testimonial.Text = model.Text;
        testimonial.AuthorName = model.AuthorName;
        testimonial.AuthorType = model.AuthorType;
        testimonial.SortOrder = model.SortOrder;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Testimonialul a fost actualizat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return NotFound();
        _context.Testimonials.Remove(testimonial);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Testimonialul a fost șters.";
        return RedirectToAction("Index");
    }
}
