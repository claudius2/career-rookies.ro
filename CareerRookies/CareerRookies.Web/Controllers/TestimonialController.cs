using Microsoft.AspNetCore.Mvc;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Controllers;

[Route("testimoniale")]
public class TestimonialController : Controller
{
    private readonly ITestimonialService _testimonialService;

    public TestimonialController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var testimonials = await _testimonialService.GetApprovedAsync();
        return View(testimonials);
    }
}
