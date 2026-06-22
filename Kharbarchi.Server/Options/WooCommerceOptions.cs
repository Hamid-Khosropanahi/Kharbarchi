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

    [Range(5, 120)]
    public int TimeoutSeconds { get; init; } = 30;

    public bool AllowInsecureLocalhostSsl { get; init; }
}
