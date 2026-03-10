using Microsoft.AspNetCore.Mvc;

namespace CareerRookies.Web.Controllers;

[Route("legal")]
public class LegalController : Controller
{
    [Route("termeni")]
    public IActionResult Terms() => View();

    [Route("confidentialitate")]
    public IActionResult Privacy() => View();

    [Route("cookies")]
    public IActionResult Cookies() => View();
}
