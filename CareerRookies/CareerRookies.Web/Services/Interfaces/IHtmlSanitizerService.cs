namespace CareerRookies.Web.Services.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitize(string html);
}
