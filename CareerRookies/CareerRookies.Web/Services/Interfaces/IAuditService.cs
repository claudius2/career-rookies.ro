namespace CareerRookies.Web.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityType, int entityId, string action, string? userEmail = null, string? details = null);
}
