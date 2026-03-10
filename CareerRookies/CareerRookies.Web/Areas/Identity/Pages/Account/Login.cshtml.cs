using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CareerRookies.Web.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Adresa de email este obligatorie.")]
        [EmailAddress(ErrorMessage = "Adresa de email nu este validă.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [DataType(DataType.Password)]
        [Display(Name = "Parolă")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ține-mă minte")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("Utilizatorul {Email} s-a autentificat.", Input.Email);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Contul {Email} a fost blocat.", Input.Email);
                ModelState.AddModelError(string.Empty, "Contul a fost temporar blocat. Încearcă din nou mai târziu.");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email sau parolă incorectă.");
                return Page();
            }
        }

        return Page();
    }
}
