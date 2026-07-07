using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Kharbarchi.Server.Services;

public static class WooCommerceRequestSecurity
{
    private const string RedactedValue = "[REDACTED]";

    private static readonly Regex CredentialQueryPattern = new(
        @"(?<name>consumer_(?:key|secret))=(?<value>[^&\s""']*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex AuthorizationPattern = new(
        @"Authorization\s*:\s*Basic\s+[A-Za-z0-9+/=]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static void ApplyBasicAuthentication(
        HttpRequestMessage request,
        string? consumerKey,
        string? consumerSecret)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
        {
            throw new InvalidOperationException("WooCommerce consumer credentials are not configured.");
        }

        var rawToken = $"{consumerKey}:{consumerSecret}";
        var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", encodedToken);
    }

    public static void RejectCredentialQueryParameters(string? url)
    {
        if (!string.IsNullOrWhiteSpace(url) && CredentialQueryPattern.IsMatch(url))
        {
            throw new InvalidOperationException(
                "WooCommerce credentials must be sent with the Authorization header, not in the request URL.");
        }
    }

    public static string Sanitize(
        string? value,
        string? consumerKey,
        string? consumerSecret,
        int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sanitized = CredentialQueryPattern.Replace(
            value,
            match => $"{match.Groups["name"].Value}={RedactedValue}");
        sanitized = AuthorizationPattern.Replace(sanitized, $"Authorization: Basic {RedactedValue}");

        foreach (var sensitiveValue in GetSensitiveValues(consumerKey, consumerSecret))
        {
            sanitized = sanitized.Replace(sensitiveValue, RedactedValue, StringComparison.Ordinal);
        }

        return sanitized.Length <= maxLength
            ? sanitized
            : sanitized[..maxLength] + "...";
    }

    private static IEnumerable<string> GetSensitiveValues(string? consumerKey, string? consumerSecret)
    {
        if (!string.IsNullOrWhiteSpace(consumerKey))
        {
            yield return consumerKey;
            yield return Uri.EscapeDataString(consumerKey);
        }

        if (!string.IsNullOrWhiteSpace(consumerSecret))
        {
            yield return consumerSecret;
            yield return Uri.EscapeDataString(consumerSecret);
        }

        if (!string.IsNullOrWhiteSpace(consumerKey) && !string.IsNullOrWhiteSpace(consumerSecret))
        {
            var encodedToken = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
            yield return encodedToken;
            yield return Uri.EscapeDataString(encodedToken);
        }
    }
}
