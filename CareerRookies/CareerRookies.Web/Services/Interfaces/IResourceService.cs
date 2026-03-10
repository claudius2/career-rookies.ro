using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services.Interfaces;

public interface IResourceService
{
    Task<Dictionary<ResourceCategory, List<CareerResource>>> GetGroupedAsync();
    Task<List<CareerResource>> GetTopAsync(int count);
    Task<PagedResult<CareerResource>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<CareerResource?> GetByIdAsync(int id);
    Task<CareerResource> CreateAsync(CareerResource resource);
    Task UpdateAsync(CareerResource resource);
    Task DeleteAsync(int id);
}
