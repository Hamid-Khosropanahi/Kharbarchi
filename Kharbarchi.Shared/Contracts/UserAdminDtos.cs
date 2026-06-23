using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Contracts;

public sealed record LocalRoleDto(
    string Name,
    string PersianName,
    string Description);

public sealed record LocalUserDto(
    string Id,
    string UserName,
    string? Email,
    string? FullName,
    bool IsLockedOut,
    IReadOnlyList<string> Roles);

public sealed record CreateLocalUserRequest
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [RegularExpression("^[A-Za-z0-9._-]{3,80}$", ErrorMessage = "نام کاربری فقط می‌تواند شامل حروف انگلیسی، عدد، نقطه، خط تیره و زیرخط باشد.")]
    public string UserName { get; init; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    [StringLength(320)]
    public string? Email { get; init; }

    [StringLength(160)]
    public string? FullName { get; init; }

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "رمز عبور باید حداقل ۱۰ کاراکتر باشد.")]
    public string Password { get; init; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "تکرار رمز عبور با رمز اصلی یکی نیست.")]
    public string ConfirmPassword { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = [];
}

public sealed record UpdateLocalUserRolesRequest
{
    public IReadOnlyList<string> Roles { get; init; } = [];
}

public sealed record ResetLocalUserPasswordRequest
{
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string NewPassword { get; init; } = string.Empty;

    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

public sealed record SetLocalUserLockStateRequest
{
    public bool IsLockedOut { get; init; }
}

public sealed record UserAdminOperationResult(
    bool IsSuccess,
    string Message);
