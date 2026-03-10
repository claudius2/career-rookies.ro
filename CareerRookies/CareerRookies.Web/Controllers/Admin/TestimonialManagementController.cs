using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels.Admin;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Testimonials")]
public class TestimonialManagementController : Controller
{
    private readonly ITestimonialService _testimonialService;
    private readonly IAuditService _auditService;

    public TestimonialManagementController(ITestimonialService testimonialService, IAuditService auditService)
    {
        _testimonialService = testimonialService;
        _auditService = auditService;
    }

    [Route("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var testimonials = await _testimonialService.GetAllAsync(page);
        return View("~/Views/Admin/Testimonial/Index.cshtml", testimonials);
    }

    [Route("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Testimonial/Create.cshtml", new TestimonialFormViewModel());
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TestimonialFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Testimonial/Create.cshtml", model);

        var testimonial = new Testimonial
        {
            Text = model.Text,
            AuthorName = model.AuthorName,
            AuthorType = model.AuthorType,
            IsApproved = model.IsApproved,
            SortOrder = model.SortOrder
        };

        await _testimonialService.CreateAsync(testimonial);
        await _auditService.LogAsync("Testimonial", testimonial.Id, "Created", User.Identity?.Name);
        TempData["Success"] = "Testimonialul a fost creat.";
        return RedirectToAction("Index");
    }

    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var testimonial = await _testimonialService.GetByIdAsync(id);
        if (testimonial == null) return NotFound();

        var model = new TestimonialFormViewModel
        {
            Id = testimonial.Id,
            Text = testimonial.Text,
            AuthorName = testimonial.AuthorName,
            AuthorType = testimonial.AuthorType,
            IsApproved = testimonial.IsApproved,
            SortOrder = testimonial.SortOrder
        };

        return View("~/Views/Admin/Testimonial/Edit.cshtml", model);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TestimonialFormViewModel model)
    {
        if (id != model.Id) return NotFound();
        var testimonial = await _testimonialService.GetByIdAsync(id);
        if (testimonial == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Testimonial/Edit.cshtml", model);

        testimonial.Text = model.Text;
        testimonial.AuthorName = model.AuthorName;
        testimonial.AuthorType = model.AuthorType;
        testimonial.IsApproved = model.IsApproved;
        testimonial.SortOrder = model.SortOrder;
        await _testimonialService.UpdateAsync(testimonial);
        await _auditService.LogAsync("Testimonial", testimonial.Id, "Updated", User.Identity?.Name);
        TempData["Success"] = "Testimonialul a fost actualizat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Approve/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        await _testimonialService.ApproveAsync(id);
        await _auditService.LogAsync("Testimonial", id, "Approved", User.Identity?.Name);
        TempData["Success"] = "Testimonialul a fost aprobat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _testimonialService.SoftDeleteAsync(id);
        await _auditService.LogAsync("Testimonial", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Testimonialul a fost sters.";
        return RedirectToAction("Index");
    }
}
