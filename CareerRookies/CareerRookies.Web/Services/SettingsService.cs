using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Services;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;

    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SiteSetting>> GetAllAsync()
    {
        return await _context.SiteSettings.OrderBy(s => s.Key).ToListAsync();
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<Dictionary<string, string>> GetMultipleAsync(params string[] keys)
    {
        var settings = await _context.SiteSettings
            .Where(s => keys.Contains(s.Key))
            .ToListAsync();
        return settings.ToDictionary(s => s.Key, s => s.Value);
    }

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var value = await GetValueAsync(key);
        return value != null ? bool.TryParse(value, out var result) && result : defaultValue;
    }

    public async Task UpdateAsync(Dictionary<string, string> settings)
    {
        var allSettings = await _context.SiteSettings.ToListAsync();
        var settingsDict = allSettings.ToDictionary(s => s.Key, s => s);

        foreach (var kvp in settings)
        {
            if (settingsDict.TryGetValue(kvp.Key, out var setting))
            {
                setting.Value = kvp.Value;
            }
        }

        await _context.SaveChangesAsync();
    }
}
