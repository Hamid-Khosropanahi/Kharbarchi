namespace Kharbarchi.Server.Options;

public sealed class BootstrapSuperAdminOptions
{
    public const string SectionName = "BootstrapSuperAdmin";

    public bool Enabled { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public bool ResetPasswordIfExists { get; set; }
}
