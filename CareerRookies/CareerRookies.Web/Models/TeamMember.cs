using System.ComponentModel.DataAnnotations;
using CareerRookies.Web.Models.Interfaces;

namespace CareerRookies.Web.Models;

public class TeamMember : ITimestamped
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Display(Name = "Descriere")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Imagine")]
    public string? ImagePath { get; set; }

    [Display(Name = "Ordine")]
    public int SortOrder { get; set; }

    [Display(Name = "Activ")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
