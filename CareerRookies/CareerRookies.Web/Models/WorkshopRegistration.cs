using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.Models;

public class WorkshopRegistration
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Workshop")]
    public int WorkshopId { get; set; }

    [Required(ErrorMessage = "Numele elevului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume elev")]
    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Clasa este obligatorie.")]
    [Display(Name = "Clasa")]
    public int StudentClassId { get; set; }

    [Required(ErrorMessage = "Consimțământul GDPR este obligatoriu.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Trebuie să accepți termenii GDPR.")]
    [Display(Name = "Consimțământ GDPR")]
    public bool GdprConsentAccepted { get; set; }

    [Display(Name = "Document GDPR")]
    public string? GdprDocumentPath { get; set; }

    [Display(Name = "Înregistrat la")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Workshop Workshop { get; set; } = null!;
    public StudentClass StudentClass { get; set; } = null!;
}
