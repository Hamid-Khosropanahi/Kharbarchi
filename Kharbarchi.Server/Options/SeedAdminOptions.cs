namespace Kharbarchi.Server.Options;

public sealed class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public bool Enabled { get; init; }
    public string UserName { get; init; } = "admin";
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = "Kharbarchi Admin";
    public string Password { get; init; } = string.Empty;
}
