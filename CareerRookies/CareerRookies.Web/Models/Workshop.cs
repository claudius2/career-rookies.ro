using System.ComponentModel.DataAnnotations;
using CareerRookies.Web.Models.Interfaces;

namespace CareerRookies.Web.Models;

public class Workshop : ITimestamped, ISoftDeletable
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(250)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea este obligatorie.")]
    [Display(Name = "Descriere")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data este obligatorie.")]
    [Display(Name = "Data")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }

    [Display(Name = "Capacitate maxima")]
    [Range(0, 10000, ErrorMessage = "Capacitatea trebuie sa fie intre 0 si 10000.")]
    public int? MaxCapacity { get; set; }

    [Display(Name = "Imagine")]
    public string? ImagePath { get; set; }

    [Display(Name = "Creat la")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Actualizat la")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<WorkshopSpeaker> WorkshopSpeakers { get; set; } = new List<WorkshopSpeaker>();
    public ICollection<WorkshopRegistration> Registrations { get; set; } = new List<WorkshopRegistration>();
    public ICollection<WorkshopMedia> Media { get; set; } = new List<WorkshopMedia>();

    /// <summary>Helper to get speaker names as a comma-separated string.</summary>
    public string SpeakerNames => WorkshopSpeakers.Any()
        ? string.Join(", ", WorkshopSpeakers.OrderBy(ws => ws.SortOrder).Select(ws => ws.Speaker.Name))
        : string.Empty;

    public int RegistrationCount => Registrations.Count;
    public bool IsFull => MaxCapacity.HasValue && RegistrationCount >= MaxCapacity.Value;
}
