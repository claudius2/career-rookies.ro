namespace CareerRookies.Web.Services.Interfaces;

public interface IRecaptchaService
{
    Task<bool> VerifyAsync(string? recaptchaResponse);
    string SiteKey { get; }
    bool IsEnabled { get; }
}
