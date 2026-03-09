using Microsoft.AspNetCore.Mvc;

namespace CareerRookies.Web.Controllers;

public class LegalController : Controller
{
    public IActionResult Terms() => View();
    public IActionResult Privacy() => View();
    public IActionResult Cookies() => View();
}
