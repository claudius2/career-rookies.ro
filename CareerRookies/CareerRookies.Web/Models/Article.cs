using System.ComponentModel.DataAnnotations;
using CareerRookies.Web.Models.Interfaces;

namespace CareerRookies.Web.Models;

public class Article : ITimestamped, ISoftDeletable
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(250)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Continutul este obligatoriu.")]
    [MaxLength(50000)]
    [Display(Name = "Continut")]
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

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
