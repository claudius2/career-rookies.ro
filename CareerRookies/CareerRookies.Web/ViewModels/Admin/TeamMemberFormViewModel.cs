using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.ViewModels.Admin;

public class TeamMemberFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000)]
    [Display(Name = "Descriere")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Imagine")]
    public IFormFile? Image { get; set; }

    public string? ExistingImagePath { get; set; }

    [Display(Name = "Ordine")]
    public int SortOrder { get; set; }

    [Display(Name = "Activ")]
    public bool IsActive { get; set; } = true;
}
