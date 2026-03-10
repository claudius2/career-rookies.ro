using System.Text.RegularExpressions;

namespace CareerRookies.Web.Services;

public static partial class TextHelper
{
    public static string StripTags(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        return TagRegex().Replace(html, " ").Trim();
    }

    public static string Truncate(string? html, int maxLength)
    {
        var text = StripTags(html);
        // Collapse multiple spaces
        text = MultiSpaceRegex().Replace(text, " ");
        return text.Length > maxLength ? text[..maxLength] + "..." : text;
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpaceRegex();
}
