using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Server.Options;

public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";

    [Required]
    public string AllowedUserName { get; init; } = "gateway_admin";

    public string RequiredAdminRole { get; init; } = "SuperAdmin";
    public string RequiredGatewayRole { get; init; } = "GatewayAdmin";
    public bool EnforceAllowedUserName { get; init; } = true;

    // Only for first local setup. Keep false in production and set values through User Secrets, not appsettings.json.
    public bool SeedGatewayUser { get; init; }
    public string Email { get; init; } = "gateway_admin@kharbarchi.local";
    public string FullName { get; init; } = "Kharbarchi Gateway Integration";
    public string Password { get; init; } = string.Empty;
}
