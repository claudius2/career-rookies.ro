using CareerRookies.Web.Models;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services.Interfaces;

public interface IWorkshopService
{
    Task<PagedResult<Workshop>> GetUpcomingAsync(int page = 1, int pageSize = 10);
    Task<PagedResult<Workshop>> GetPastAsync(int page = 1, int pageSize = 10);
    Task<Workshop?> GetByIdAsync(int id);
    Task<Workshop?> GetBySlugAsync(string slug);
    Task<Workshop?> GetWithMediaAsync(int id);
    Task<Workshop?> GetWithRegistrationsAsync(int id);
    Task<List<Workshop>> GetUpcomingTopAsync(int count);
    Task<PagedResult<Workshop>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Workshop> CreateAsync(Workshop workshop);
    Task UpdateAsync(Workshop workshop);
    Task SoftDeleteAsync(int id);
    Task<bool> CanRegisterAsync(int workshopId, string studentName, int studentClassId);
    Task<WorkshopRegistration> RegisterAsync(WorkshopRegistration registration);
    Task<int> GetRegistrationCountAsync(int workshopId);
    Task<WorkshopMedia> AddMediaAsync(WorkshopMedia media);
    Task<WorkshopMedia?> GetMediaByIdAsync(int mediaId);
    Task DeleteMediaAsync(int mediaId);
    Task<int> GetNextMediaSortOrderAsync(int workshopId);
    Task<byte[]> ExportRegistrationsCsvAsync(int workshopId);
}
