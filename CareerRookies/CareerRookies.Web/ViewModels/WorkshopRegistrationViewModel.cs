using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.ViewModels;

public class WorkshopRegistrationViewModel
{
    public int WorkshopId { get; set; }

    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume complet")]
    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Clasa este obligatorie.")]
    [Display(Name = "Clasa")]
    public string StudentClass { get; set; } = string.Empty;

    [Required(ErrorMessage = "Trebuie să accepți termenii GDPR.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Trebuie să accepți termenii GDPR.")]
    [Display(Name = "Accept prelucrarea datelor personale conform GDPR")]
    public bool GdprConsentAccepted { get; set; }
}
