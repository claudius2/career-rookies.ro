using System.ComponentModel.DataAnnotations;

namespace CareerRookies.Web.Models;

public class WorkshopMedia
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Workshop")]
    public int WorkshopId { get; set; }

    [Required(ErrorMessage = "Calea fisierului este obligatorie.")]
    [Display(Name = "Cale fisier")]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tip media")]
    public MediaType MediaType { get; set; }

    [Display(Name = "Ordine")]
    public int SortOrder { get; set; }

    public Workshop Workshop { get; set; } = null!;
}
