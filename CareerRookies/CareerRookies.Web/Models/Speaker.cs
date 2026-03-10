using System.ComponentModel.DataAnnotations;
using CareerRookies.Web.Models.Interfaces;

namespace CareerRookies.Web.Models;

public class Speaker : ITimestamped
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele speaker-ului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea speaker-ului este obligatorie.")]
    [Display(Name = "Descriere")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Imagine")]
    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkshopSpeaker> WorkshopSpeakers { get; set; } = new List<WorkshopSpeaker>();
}
