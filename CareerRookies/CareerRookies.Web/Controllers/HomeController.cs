using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

public class HomeController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly ITestimonialService _testimonialService;
    private readonly IArticleService _articleService;
    private readonly IResourceService _resourceService;

    public HomeController(
        IWorkshopService workshopService,
        ITestimonialService testimonialService,
        IArticleService articleService,
        IResourceService resourceService)
    {
        _workshopService = workshopService;
        _testimonialService = testimonialService;
        _articleService = articleService;
        _resourceService = resourceService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            UpcomingWorkshops = await _workshopService.GetUpcomingTopAsync(3),
            Testimonials = await _testimonialService.GetTopApprovedAsync(10),
            RecentArticles = await _articleService.GetRecentApprovedAsync(3),
            FeaturedResources = await _resourceService.GetTopAsync(6)
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    public async Task<IActionResult> Search(string? q, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(new PagedResult<Models.Article>());

        var results = await _articleService.SearchAsync(q.Trim(), page);
        ViewBag.Query = q;
        return View(results);
    }
}
