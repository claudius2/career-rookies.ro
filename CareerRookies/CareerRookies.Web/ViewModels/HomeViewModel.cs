using CareerRookies.Web.Models;

namespace CareerRookies.Web.ViewModels;

public class HomeViewModel
{
    public List<Workshop> UpcomingWorkshops { get; set; } = new();
    public List<Testimonial> Testimonials { get; set; } = new();
    public List<Article> RecentArticles { get; set; } = new();
    public List<CareerResource> FeaturedResources { get; set; } = new();
}
