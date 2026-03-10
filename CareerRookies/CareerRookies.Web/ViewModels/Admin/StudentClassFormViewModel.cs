using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.ViewModels.Admin;

public class StudentClassFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele clasei este obligatoriu.")]
    [MaxLength(50)]
    [Display(Name = "Nume clasa")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Activ")]
    public bool IsActive { get; set; } = true;
}
