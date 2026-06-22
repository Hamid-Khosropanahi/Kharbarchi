using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Contracts;

public sealed record LocalUserDto(
    string Id,
    string UserName,
    string? Email,
    string? FullName,
    bool IsLockedOut,
    IReadOnlyList<string> Roles);

public sealed record CreateLocalUserRequest
{
    [Required, StringLength(80, MinimumLength = 3)]
    public string UserName { get; init; } = string.Empty;

    [EmailAddress, StringLength(320)]
    public string? Email { get; init; }

    [StringLength(160)]
    public string? FullName { get; init; }

    [Required, StringLength(200, MinimumLength = 10)]
    public string Password { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = [];
}

public sealed record UpdateLocalUserRolesRequest
{
    public IReadOnlyList<string> Roles { get; init; } = [];
}
