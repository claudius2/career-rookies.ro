using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

[Route("articole")]
public class ArticleController : Controller
{
    private readonly IArticleService _articleService;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IRecaptchaService _recaptchaService;

    public ArticleController(IArticleService articleService, IHtmlSanitizerService htmlSanitizer, IRecaptchaService recaptchaService)
    {
        _articleService = articleService;
        _htmlSanitizer = htmlSanitizer;
        _recaptchaService = recaptchaService;
    }

    [Route("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var articles = await _articleService.GetApprovedAsync(page);
        return View(articles);
    }

    [Route("detalii/{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var article = await _articleService.GetApprovedByIdAsync(id);
        if (article == null) return NotFound();
        return View(article);
    }

    [Route("{slug}")]
    public async Task<IActionResult> DetailBySlug(string slug)
    {
        var article = await _articleService.GetBySlugAsync(slug);
        if (article == null) return NotFound();
        return View("Detail", article);
    }

    [Route("trimite")]
    public IActionResult Submit()
    {
        ViewBag.RecaptchaSiteKey = _recaptchaService.SiteKey;
        ViewBag.RecaptchaEnabled = _recaptchaService.IsEnabled;
        return View(new ArticleSubmitViewModel());
    }

    [HttpPost]
    [Route("trimite")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ArticleSubmitViewModel model)
    {
        // Verify reCAPTCHA
        var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptchaService.VerifyAsync(recaptchaResponse))
        {
            ModelState.AddModelError(string.Empty, "Verificarea reCAPTCHA a eșuat. Încearcă din nou.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.RecaptchaSiteKey = _recaptchaService.SiteKey;
            ViewBag.RecaptchaEnabled = _recaptchaService.IsEnabled;
            return View(model);
        }

        var sanitizedContent = _htmlSanitizer.Sanitize(model.Content);
        await _articleService.SubmitAsync(model.Title, sanitizedContent, model.AuthorName);

        TempData["Success"] = "Articolul a fost trimis cu succes! Echipa noastră îl va revizui în curând.";
        return RedirectToAction("Submit");
    }
}
