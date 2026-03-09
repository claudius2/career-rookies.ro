using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.Models;

public class Workshop
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea este obligatorie.")]
    [Display(Name = "Descriere")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data este obligatorie.")]
    [Display(Name = "Data")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Numele speaker-ului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume speaker")]
    public string SpeakerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea speaker-ului este obligatorie.")]
    [Display(Name = "Descriere speaker")]
    public string SpeakerDescription { get; set; } = string.Empty;

    [Display(Name = "Imagine speaker")]
    public string? SpeakerImagePath { get; set; }

    [Display(Name = "Imagine")]
    public string? ImagePath { get; set; }

    [Display(Name = "Creat la")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Actualizat la")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkshopRegistration> Registrations { get; set; } = new List<WorkshopRegistration>();

    public ICollection<WorkshopMedia> Media { get; set; } = new List<WorkshopMedia>();
}
