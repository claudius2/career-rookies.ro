using CareerRookies.Web.Models;

namespace CareerRookies.Web.ViewModels;

public class WorkshopDetailViewModel
{
    public Workshop Workshop { get; set; } = null!;
    public bool IsUpcoming => Workshop.Date > DateTime.UtcNow;
    public bool IsFull => Workshop.MaxCapacity.HasValue && RegistrationCount >= Workshop.MaxCapacity.Value;
    public int RegistrationCount { get; set; }
    public List<WorkshopMedia> Media { get; set; } = new();
    public WorkshopRegistrationViewModel? Registration { get; set; }
    public List<StudentClass> StudentClasses { get; set; } = new();
}
