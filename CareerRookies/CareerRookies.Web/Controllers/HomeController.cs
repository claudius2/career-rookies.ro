using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly ITestimonialService _testimonialService;
    private readonly IArticleService _articleService;
    private readonly IResourceService _resourceService;
    private readonly ISettingsService _settingsService;
    private readonly ITeamMemberService _teamMemberService;

    public HomeController(
        IWorkshopService workshopService,
        ITestimonialService testimonialService,
        IArticleService articleService,
        IResourceService resourceService,
        ISettingsService settingsService,
        ITeamMemberService teamMemberService)
    {
        _workshopService = workshopService;
        _testimonialService = testimonialService;
        _articleService = articleService;
        _resourceService = resourceService;
        _settingsService = settingsService;
        _teamMemberService = teamMemberService;
    }

    [Route("")]
    [Route("acasa")]
    public async Task<IActionResult> Index()
    {
        var aboutText = await _settingsService.GetValueAsync("AboutProjectText");
        var model = new HomeViewModel
        {
            UpcomingWorkshops = await _workshopService.GetUpcomingTopAsync(3),
            Testimonials = await _testimonialService.GetTopApprovedAsync(10),
            RecentArticles = await _articleService.GetRecentApprovedAsync(3),
            FeaturedResources = await _resourceService.GetTopAsync(6),
            AboutProjectText = aboutText ?? "<p>Career Rookies este o inițiativă dedicată elevilor din România care își doresc să-și construiască un viitor profesional de succes.</p>",
            TeamMembers = await _teamMemberService.GetActiveAsync()
        };
        return View(model);
    }

    [Route("eroare")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    [Route("cauta")]
    public async Task<IActionResult> Search(string? q, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(new PagedResult<Models.Article>());

        var results = await _articleService.SearchAsync(q.Trim(), page);
        ViewBag.Query = q;
        return View(results);
    }
}
