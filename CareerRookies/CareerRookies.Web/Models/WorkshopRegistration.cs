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
    [MaxLength(50)]
    [Display(Name = "Clasa")]
    public string StudentClass { get; set; } = string.Empty;

    [Required(ErrorMessage = "Consimtamantul GDPR este obligatoriu.")]
    [Display(Name = "Consimtamant GDPR")]
    public bool GdprConsentAccepted { get; set; }

    [Display(Name = "Document GDPR")]
    public string? GdprDocumentPath { get; set; }

    [Display(Name = "Inregistrat la")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Workshop Workshop { get; set; } = null!;
}
