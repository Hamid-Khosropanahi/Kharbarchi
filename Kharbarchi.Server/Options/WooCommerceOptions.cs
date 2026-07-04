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

    // New explicit fields
    // "Local" or "Production" (case-insensitive). Default: Local
    public string EnvironmentType { get; set; } = "Local";

    // Whether SSL should be verified when contacting WooCommerce. Default: true
    public bool VerifySsl { get; set; } = true;

    [Range(5, 120)]
    public int TimeoutSeconds { get; init; } = 30;

    public bool AllowInsecureLocalhostSsl { get; init; }
}
