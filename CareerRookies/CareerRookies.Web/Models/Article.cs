using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.Models;

public class Article
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Conținutul este obligatoriu.")]
    [MaxLength(50000)]
    [Display(Name = "Conținut")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele autorului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume autor")]
    public string AuthorName { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public ArticleStatus Status { get; set; } = ArticleStatus.Pending;

    [Display(Name = "Creat la")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Actualizat la")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
