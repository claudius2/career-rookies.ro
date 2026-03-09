using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.Models;

public class SiteSetting
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Cheia este obligatorie.")]
    [MaxLength(100)]
    [Display(Name = "Cheie")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valoarea este obligatorie.")]
    [MaxLength(500)]
    [Display(Name = "Valoare")]
    public string Value { get; set; } = string.Empty;
}
