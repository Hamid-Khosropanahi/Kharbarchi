using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Server.Options;

public sealed class WooCommerceOptions
{
    public const string SectionName = "WooCommerce";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string ConsumerKey { get; init; } = string.Empty;

    [Required]
    public string ConsumerSecret { get; init; } = string.Empty;

    // Must be explicitly configured as "Local" or "Production".
    // An empty default prevents Production from silently inheriting a Local profile.
    public string EnvironmentType { get; init; } = string.Empty;

    // Whether SSL should be verified when contacting WooCommerce.
    public bool VerifySsl { get; init; } = true;

    [Range(5, 120)]
    public int TimeoutSeconds { get; init; } = 30;

    public bool AllowInsecureLocalhostSsl { get; init; }
}
