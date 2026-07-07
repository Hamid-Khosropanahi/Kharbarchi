namespace Kharbarchi.Shared.Auth;

public sealed record CurrentUserProfileDto
{
    public string UserName { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<SafeUserClaimDto> Claims { get; init; } = [];
}

public sealed record SafeUserClaimDto(string Type, string Value);
