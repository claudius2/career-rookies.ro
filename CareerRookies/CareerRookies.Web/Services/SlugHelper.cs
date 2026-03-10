using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CareerRookies.Web.Services;

public static partial class SlugHelper
{
    private static readonly Dictionary<char, string> RomanianMap = new()
    {
        ['ă'] = "a", ['â'] = "a", ['î'] = "i", ['ș'] = "s", ['ț'] = "t",
        ['Ă'] = "a", ['Â'] = "a", ['Î'] = "i", ['Ș'] = "s", ['Ț'] = "t"
    };

    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (RomanianMap.TryGetValue(c, out var replacement))
                sb.Append(replacement);
            else
                sb.Append(c);
        }

        var normalized = sb.ToString()
            .Normalize(NormalizationForm.FormD);

        var result = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                result.Append(c);
        }

        var slug = result.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        slug = NonAlphanumericRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug.Length > 200 ? slug[..200].TrimEnd('-') : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
