using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CareerRookies.Web.ViewModels.Admin;

public class WorkshopFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [MaxLength(200)]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea este obligatorie.")]
    [Display(Name = "Descriere")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data este obligatorie.")]
    [Display(Name = "Data")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }

    [Display(Name = "Capacitate maxima")]
    [Range(0, 10000, ErrorMessage = "Capacitatea trebuie sa fie intre 0 si 10000.")]
    public int? MaxCapacity { get; set; }

    [Display(Name = "Imagine workshop")]
    public IFormFile? Image { get; set; }

    public string? ExistingImagePath { get; set; }

    public List<SpeakerFormItem> Speakers { get; set; } = new();
}

public class SpeakerFormItem
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Numele speaker-ului este obligatoriu.")]
    [MaxLength(100)]
    [Display(Name = "Nume speaker")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea speaker-ului este obligatorie.")]
    [Display(Name = "Descriere speaker")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Imagine speaker")]
    public IFormFile? Image { get; set; }

    public string? ExistingImagePath { get; set; }
}
