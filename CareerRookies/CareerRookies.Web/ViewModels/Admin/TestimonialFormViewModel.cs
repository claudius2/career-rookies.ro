using System.ComponentModel.DataAnnotations;
using CareerRookies.Web.Models;

namespace CareerRookies.Web.ViewModels.Admin;

public class TestimonialFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Textul este obligatoriu.")]
    [MaxLength(1000)]
    [Display(Name = "Text")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele autorului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume autor")]
    public string AuthorName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tip autor")]
    public AuthorType AuthorType { get; set; }

    [Display(Name = "Aprobat")]
    public bool IsApproved { get; set; }

    [Display(Name = "Ordine")]
    public int SortOrder { get; set; }
}
