using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services;

public class ResourceService : IResourceService
{
    private readonly ApplicationDbContext _context;

    public ResourceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<ResourceCategory, List<CareerResource>>> GetGroupedAsync()
    {
        var resources = await _context.CareerResources
            .OrderBy(r => r.Category)
            .ThenBy(r => r.SortOrder)
            .ToListAsync();

        return resources.GroupBy(r => r.Category).ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<List<CareerResource>> GetTopAsync(int count)
    {
        return await _context.CareerResources
            .OrderBy(r => r.SortOrder)
            .Take(count)
            .ToListAsync();
    }

    public async Task<PagedResult<CareerResource>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.CareerResources
            .OrderBy(r => r.Category)
            .ThenBy(r => r.SortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CareerResource>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CareerResource?> GetByIdAsync(int id)
    {
        return await _context.CareerResources.FindAsync(id);
    }

    public async Task<CareerResource> CreateAsync(CareerResource resource)
    {
        _context.CareerResources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    public async Task UpdateAsync(CareerResource resource)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var resource = await _context.CareerResources.FindAsync(id);
        if (resource == null) return;
        _context.CareerResources.Remove(resource);
        await _context.SaveChangesAsync();
    }
}
