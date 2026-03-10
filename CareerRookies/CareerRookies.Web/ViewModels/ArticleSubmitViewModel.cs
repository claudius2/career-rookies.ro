using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.ViewModels;

public class ArticleSubmitViewModel
{
    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Conținutul este obligatoriu.")]
    [MaxLength(50000, ErrorMessage = "Conținutul nu poate depăși 50000 de caractere.")]
    [Display(Name = "Conținut")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele autorului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Numele autorului")]
    public string AuthorName { get; set; } = string.Empty;
}
