using CareerRookies.Web.Models;

namespace CareerRookies.Web.Services.Interfaces;

public interface ISettingsService
{
    Task<List<SiteSetting>> GetAllAsync();
    Task<string?> GetValueAsync(string key);
    Task<bool> GetBoolAsync(string key, bool defaultValue = false);
    Task UpdateAsync(Dictionary<string, string> settings);
}
