using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Articles")]
public class ArticleManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArticleManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var articles = await _context.Articles
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return View("~/Views/Admin/Article/Index.cshtml", articles);
    }

    [HttpPost]
    [Route("Approve/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return NotFound();
        article.IsApproved = true;
        article.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Articolul a fost aprobat.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Reject/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return NotFound();
        article.IsApproved = false;
        article.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Articolul a fost respins.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return NotFound();
        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Articolul a fost șters.";
        return RedirectToAction("Index");
    }
}
