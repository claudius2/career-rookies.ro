using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services;

public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;

    public ArticleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Article>> GetApprovedAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Articles
            .Where(a => a.Status == ArticleStatus.Approved && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt);

        return await PaginateAsync(query, page, pageSize);
    }

    public async Task<Article?> GetByIdAsync(int id)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<Article?> GetBySlugAsync(string slug)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == ArticleStatus.Approved && !a.IsDeleted);
    }

    public async Task<Article?> GetApprovedByIdAsync(int id)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Approved && !a.IsDeleted);
    }

    public async Task<List<Article>> GetRecentApprovedAsync(int count)
    {
        return await _context.Articles
            .Where(a => a.Status == ArticleStatus.Approved && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<PagedResult<Article>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.Articles
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt);

        return await PaginateAsync(query, page, pageSize);
    }

    public async Task<Article> SubmitAsync(string title, string content, string authorName)
    {
        var article = new Article
        {
            Title = title,
            Slug = SlugHelper.GenerateSlug(title),
            Content = content,
            AuthorName = authorName,
            Status = ArticleStatus.Pending
        };

        // Ensure unique slug
        article.Slug = await EnsureUniqueSlugAsync(article.Slug, 0);

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task ApproveAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return;
        article.Status = ArticleStatus.Approved;
        await _context.SaveChangesAsync();
    }

    public async Task RejectAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return;
        article.Status = ArticleStatus.Rejected;
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return;
        article.IsDeleted = true;
        article.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Article>> SearchAsync(string query, int page = 1, int pageSize = 10)
    {
        var q = _context.Articles
            .Where(a => a.Status == ArticleStatus.Approved && !a.IsDeleted)
            .Where(a => a.Title.Contains(query) || a.Content.Contains(query) || a.AuthorName.Contains(query))
            .OrderByDescending(a => a.CreatedAt);

        return await PaginateAsync(q, page, pageSize);
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int excludeId)
    {
        var baseSlug = slug;
        var counter = 1;
        while (await _context.Articles.AnyAsync(a => a.Slug == slug && a.Id != excludeId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        return slug;
    }

    private static async Task<PagedResult<T>> PaginateAsync<T>(IOrderedQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
