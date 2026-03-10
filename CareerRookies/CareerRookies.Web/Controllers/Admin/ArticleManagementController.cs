using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Articles")]
public class ArticleManagementController : Controller
{
    private readonly IArticleService _articleService;
    private readonly IAuditService _auditService;

    public ArticleManagementController(IArticleService articleService, IAuditService auditService)
    {
        _articleService = articleService;
        _auditService = auditService;
    }

    [Route("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var articles = await _articleService.GetAllAsync(page);
        return View("~/Views/Admin/Article/Index.cshtml", articles);
    }

    [HttpPost]
    [Route("Approve/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        await _articleService.ApproveAsync(id);
        await _auditService.LogAsync("Article", id, "Approved", User.Identity?.Name);
        TempData["Success"] = "Articolul a fost aprobat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Reject/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        await _articleService.RejectAsync(id);
        await _auditService.LogAsync("Article", id, "Rejected", User.Identity?.Name);
        TempData["Success"] = "Articolul a fost respins.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _articleService.SoftDeleteAsync(id);
        await _auditService.LogAsync("Article", id, "Deleted", User.Identity?.Name);
        TempData["Success"] = "Articolul a fost sters.";
        return RedirectToAction("Index");
    }
}
