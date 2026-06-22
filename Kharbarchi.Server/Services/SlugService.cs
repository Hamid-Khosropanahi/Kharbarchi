using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kharbarchi.Server.Services;

public static partial class SlugService
{
    public static string NormalizeSlug(string? requestedSlug, string fallbackText)
    {
        var source = string.IsNullOrWhiteSpace(requestedSlug) ? fallbackText : requestedSlug;
        source = source.Trim().ToLowerInvariant();

        var builder = new StringBuilder(source.Length);
        foreach (var ch in source.Normalize(NormalizationForm.FormD))
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_' or '/' or '\\')
            {
                builder.Append('-');
            }
        }

        var slug = DuplicateDashRegex().Replace(builder.ToString(), "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..12] : slug;
    }

    [GeneratedRegex("-+")]
    private static partial Regex DuplicateDashRegex();
}
