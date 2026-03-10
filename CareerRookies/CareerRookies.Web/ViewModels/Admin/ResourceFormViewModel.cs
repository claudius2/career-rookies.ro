using System.ComponentModel.DataAnnotations;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.ViewModels.Admin;

public class ResourceFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL-ul este obligatoriu.")]
    [MaxLength(500)]
    [Display(Name = "URL")]
    [Url(ErrorMessage = "URL-ul nu este valid.")]
    public string Url { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Categorie")]
    public ResourceCategory Category { get; set; }

    [Display(Name = "Ordine")]
    public int SortOrder { get; set; }
}
