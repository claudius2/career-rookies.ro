using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

public class ArticleController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArticleController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var articles = await _context.Articles
            .Where(a => a.IsApproved)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return View(articles);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id && a.IsApproved);
        if (article == null) return NotFound();
        return View(article);
    }

    public IActionResult Submit()
    {
        return View(new ArticleSubmitViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ArticleSubmitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var article = new Article
        {
            Title = model.Title,
            Content = model.Content,
            AuthorName = model.AuthorName,
            IsApproved = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Articolul a fost trimis și va fi revizuit de echipa noastră.";
        return RedirectToAction("Index");
    }
}
