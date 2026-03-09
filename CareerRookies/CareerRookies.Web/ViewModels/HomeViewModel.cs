using CareerRookies.Web.Models;

namespace CareerRookies.Web.ViewModels;

public class HomeViewModel
{
    public List<Workshop> UpcomingWorkshops { get; set; } = new();
    public List<Testimonial> Testimonials { get; set; } = new();
}
