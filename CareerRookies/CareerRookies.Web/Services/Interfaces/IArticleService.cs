using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services.Interfaces;

public interface IArticleService
{
    Task<PagedResult<Article>> GetApprovedAsync(int page = 1, int pageSize = 10);
    Task<Article?> GetByIdAsync(int id);
    Task<Article?> GetBySlugAsync(string slug);
    Task<Article?> GetApprovedByIdAsync(int id);
    Task<List<Article>> GetRecentApprovedAsync(int count);
    Task<PagedResult<Article>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Article> SubmitAsync(string title, string content, string authorName);
    Task ApproveAsync(int id);
    Task RejectAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<PagedResult<Article>> SearchAsync(string query, int page = 1, int pageSize = 10);
}
