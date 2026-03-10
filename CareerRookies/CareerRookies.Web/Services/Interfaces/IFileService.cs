namespace CareerRookies.Web.Services.Interfaces;

public interface IFileService
{
    Task<string?> SaveImageAsync(IFormFile file, string subfolder);
    Task<string?> SaveFileAsync(IFormFile file, string subfolder, HashSet<string> allowedExtensions);
    void DeleteFile(string? relativePath);
    bool IsValidYouTubeUrl(string url);
}
