using System.Text;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;
using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services;

public class WorkshopService : IWorkshopService
{
    private readonly ApplicationDbContext _context;

    public WorkshopService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Workshop>> GetUpcomingAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Where(w => w.Date > DateTime.UtcNow && !w.IsDeleted)
            .OrderBy(w => w.Date);

        return await PaginateAsync(query, page, pageSize);
    }

    public async Task<PagedResult<Workshop>> GetPastAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Where(w => w.Date <= DateTime.UtcNow && !w.IsDeleted)
            .OrderByDescending(w => w.Date);

        return await PaginateAsync(query, page, pageSize);
    }

    public async Task<Workshop?> GetByIdAsync(int id)
    {
        return await _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<Workshop?> GetBySlugAsync(string slug)
    {
        return await _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Include(w => w.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(w => w.Slug == slug && !w.IsDeleted);
    }

    public async Task<Workshop?> GetWithMediaAsync(int id)
    {
        return await _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Include(w => w.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<Workshop?> GetWithRegistrationsAsync(int id)
    {
        return await _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Include(w => w.Registrations)
                .ThenInclude(r => r.StudentClass)
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<List<Workshop>> GetUpcomingTopAsync(int count)
    {
        return await _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Where(w => w.Date > DateTime.UtcNow && !w.IsDeleted)
            .OrderBy(w => w.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<PagedResult<Workshop>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.Workshops
            .Include(w => w.WorkshopSpeakers.OrderBy(ws => ws.SortOrder))
                .ThenInclude(ws => ws.Speaker)
            .Where(w => !w.IsDeleted)
            .OrderByDescending(w => w.Date);

        return await PaginateAsync(query, page, pageSize);
    }

    public async Task<Workshop> CreateAsync(Workshop workshop)
    {
        workshop.Slug = SlugHelper.GenerateSlug(workshop.Title);
        workshop.Slug = await EnsureUniqueSlugAsync(workshop.Slug, 0);

        _context.Workshops.Add(workshop);
        await _context.SaveChangesAsync();
        return workshop;
    }

    public async Task UpdateAsync(Workshop workshop)
    {
        workshop.Slug = SlugHelper.GenerateSlug(workshop.Title);
        workshop.Slug = await EnsureUniqueSlugAsync(workshop.Slug, workshop.Id);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Media)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (workshop == null) return;

        workshop.IsDeleted = true;
        workshop.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanRegisterAsync(int workshopId, string studentName, int studentClassId)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Registrations)
            .FirstOrDefaultAsync(w => w.Id == workshopId && !w.IsDeleted);

        if (workshop == null) return false;
        if (workshop.Date <= DateTime.UtcNow) return false;
        if (workshop.MaxCapacity.HasValue && workshop.Registrations.Count >= workshop.MaxCapacity.Value) return false;

        var alreadyRegistered = await _context.WorkshopRegistrations
            .AnyAsync(r => r.WorkshopId == workshopId
                        && r.StudentName == studentName
                        && r.StudentClassId == studentClassId);

        return !alreadyRegistered;
    }

    public async Task<WorkshopRegistration> RegisterAsync(WorkshopRegistration registration)
    {
        _context.WorkshopRegistrations.Add(registration);
        await _context.SaveChangesAsync();
        return registration;
    }

    public async Task<int> GetRegistrationCountAsync(int workshopId)
    {
        return await _context.WorkshopRegistrations.CountAsync(r => r.WorkshopId == workshopId);
    }

    public async Task<WorkshopMedia> AddMediaAsync(WorkshopMedia media)
    {
        _context.WorkshopMedia.Add(media);
        await _context.SaveChangesAsync();
        return media;
    }

    public async Task<WorkshopMedia?> GetMediaByIdAsync(int mediaId)
    {
        return await _context.WorkshopMedia.FindAsync(mediaId);
    }

    public async Task DeleteMediaAsync(int mediaId)
    {
        var media = await _context.WorkshopMedia.FindAsync(mediaId);
        if (media == null) return;
        _context.WorkshopMedia.Remove(media);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextMediaSortOrderAsync(int workshopId)
    {
        return await _context.WorkshopMedia.CountAsync(m => m.WorkshopId == workshopId) + 1;
    }

    public async Task<byte[]> ExportRegistrationsCsvAsync(int workshopId)
    {
        var workshop = await _context.Workshops
            .Include(w => w.Registrations)
                .ThenInclude(r => r.StudentClass)
            .FirstOrDefaultAsync(w => w.Id == workshopId);

        if (workshop == null) return Array.Empty<byte>();

        var sb = new StringBuilder();
        sb.AppendLine("Nume,Clasa,Consimtamant GDPR,Data inregistrare");
        foreach (var r in workshop.Registrations.OrderBy(r => r.CreatedAt))
        {
            var name = EscapeCsvField(r.StudentName);
            var className = EscapeCsvField(r.StudentClass?.Name ?? "N/A");
            var gdpr = r.GdprConsentAccepted ? "Da" : "Nu";
            var date = r.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            sb.AppendLine($"{name},{className},{gdpr},{date}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task SetWorkshopSpeakersAsync(int workshopId, List<int> speakerIds)
    {
        var existing = await _context.WorkshopSpeakers
            .Where(ws => ws.WorkshopId == workshopId)
            .ToListAsync();

        _context.WorkshopSpeakers.RemoveRange(existing);

        for (int i = 0; i < speakerIds.Count; i++)
        {
            _context.WorkshopSpeakers.Add(new WorkshopSpeaker
            {
                WorkshopId = workshopId,
                SpeakerId = speakerIds[i],
                SortOrder = i
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Speaker> CreateSpeakerAsync(Speaker speaker)
    {
        _context.Speakers.Add(speaker);
        await _context.SaveChangesAsync();
        return speaker;
    }

    public async Task<Speaker?> GetSpeakerByNameAsync(string name)
    {
        return await _context.Speakers
            .FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task UpdateSpeakerAsync(Speaker speaker)
    {
        await _context.SaveChangesAsync();
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int excludeId)
    {
        var baseSlug = slug;
        var counter = 1;
        while (await _context.Workshops.AnyAsync(w => w.Slug == slug && w.Id != excludeId))
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
