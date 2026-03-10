using System.Text.Json;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Services;

public class RecaptchaSettings
{
    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}

public class RecaptchaService : IRecaptchaService
{
    private readonly RecaptchaSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RecaptchaService> _logger;

    public RecaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<RecaptchaService> logger)
    {
        _settings = new RecaptchaSettings();
        configuration.GetSection("Recaptcha").Bind(_settings);
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string SiteKey => _settings.SiteKey;

    public bool IsEnabled => !string.IsNullOrEmpty(_settings.SiteKey) && !string.IsNullOrEmpty(_settings.SecretKey);

    public async Task<bool> VerifyAsync(string? recaptchaResponse)
    {
        if (!IsEnabled)
            return true; // Skip validation if not configured

        if (string.IsNullOrEmpty(recaptchaResponse))
            return false;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = recaptchaResponse
                }));

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);
            return result.GetProperty("success").GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "reCAPTCHA verification failed");
            return true; // Allow through on service failure to avoid blocking users
        }
    }
}
