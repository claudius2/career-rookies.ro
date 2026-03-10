using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

public class ArticleController : Controller
{
    private readonly IArticleService _articleService;

    public ArticleController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var articles = await _articleService.GetApprovedAsync(page);
        return View(articles);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var article = await _articleService.GetApprovedByIdAsync(id);
        if (article == null) return NotFound();
        return View(article);
    }

    [Route("Article/{slug}")]
    public async Task<IActionResult> DetailBySlug(string slug)
    {
        var article = await _articleService.GetBySlugAsync(slug);
        if (article == null) return NotFound();
        return View("Detail", article);
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

        await _articleService.SubmitAsync(model.Title, model.Content, model.AuthorName);

        TempData["Success"] = "Articolul a fost trimis si va fi revizuit de echipa noastra.";
        return RedirectToAction("Index");
    }
}
