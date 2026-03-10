using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services;

public class TestimonialService : ITestimonialService
{
    private readonly ApplicationDbContext _context;

    public TestimonialService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Testimonial>> GetApprovedAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsApproved && !t.IsDeleted)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<List<Testimonial>> GetTopApprovedAsync(int count)
    {
        return await _context.Testimonials
            .Where(t => t.IsApproved && !t.IsDeleted)
            .OrderBy(t => t.SortOrder)
            .Take(count)
            .ToListAsync();
    }

    public async Task<PagedResult<Testimonial>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.Testimonials
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.SortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Testimonial>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Testimonial?> GetByIdAsync(int id)
    {
        return await _context.Testimonials
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<Testimonial> CreateAsync(Testimonial testimonial)
    {
        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync();
        return testimonial;
    }

    public async Task UpdateAsync(Testimonial testimonial)
    {
        await _context.SaveChangesAsync();
    }

    public async Task ApproveAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return;
        testimonial.IsApproved = true;
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return;
        testimonial.IsDeleted = true;
        testimonial.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
