using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services.Interfaces;

public interface ITestimonialService
{
    Task<List<Testimonial>> GetApprovedAsync();
    Task<List<Testimonial>> GetTopApprovedAsync(int count);
    Task<PagedResult<Testimonial>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Testimonial?> GetByIdAsync(int id);
    Task<Testimonial> CreateAsync(Testimonial testimonial);
    Task UpdateAsync(Testimonial testimonial);
    Task ApproveAsync(int id);
    Task SoftDeleteAsync(int id);
}
