using System.Security.Claims;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Security;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicyNames.SuperAdminOnly)]
[EnableRateLimiting("admin")]
[Route("api/admin/local-users")]
[Produces("application/json")]
public sealed class LocalUserAdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public LocalUserAdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("roles")]
    public ActionResult<IReadOnlyList<LocalRoleDto>> GetRoles()
    {
        var roles = KharbarchiRoles.InternalRoleCatalog
            .OrderBy(x => x.PersianName)
            .Select(x => new LocalRoleDto(x.Name, x.PersianName, x.Description))
            .ToList();

        return Ok(roles);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocalUserDto>>> GetUsers()
    {
        var users = _userManager.Users
            .OrderBy(x => x.UserName)
            .Take(500)
            .ToList();

        var result = new List<LocalUserDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(await MapUserAsync(user));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LocalUserDto>> CreateUser([FromBody] CreateLocalUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.Roles.Count == 0)
        {
            return BadRequest(new UserAdminOperationResult(false, "حداقل یک نقش باید انتخاب شود."));
        }

        var roleValidation = await NormalizeAndValidateRolesAsync(request.Roles);
        if (!roleValidation.IsValid)
        {
            return BadRequest(new UserAdminOperationResult(false, roleValidation.Error));
        }

        var roles = roleValidation.Roles;

        var userName = request.UserName.Trim();
        var existing = await _userManager.FindByNameAsync(userName);
        if (existing is not null)
        {
            return Conflict(new UserAdminOperationResult(false, "این نام کاربری قبلاً ثبت شده است."));
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? userName : request.FullName.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new UserAdminOperationResult(false, ToErrorText(createResult)));
        }

        var addRolesResult = await _userManager.AddToRolesAsync(user, roles);
        if (!addRolesResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(new UserAdminOperationResult(false, ToErrorText(addRolesResult)));
        }

        return CreatedAtAction(nameof(GetUsers), routeValues: null, value: await MapUserAsync(user));
    }

    [HttpPut("{userId}/roles")]
    public async Task<IActionResult> UpdateRoles(string userId, [FromBody] UpdateLocalUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new UserAdminOperationResult(false, "کاربر پیدا نشد."));
        }

        if (request.Roles.Count == 0)
        {
            return BadRequest(new UserAdminOperationResult(false, "حداقل یک نقش باید انتخاب شود."));
        }

        var roleValidation = await NormalizeAndValidateRolesAsync(request.Roles);
        if (!roleValidation.IsValid)
        {
            return BadRequest(new UserAdminOperationResult(false, roleValidation.Error));
        }

        var targetRoles = roleValidation.Roles;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal) &&
            !targetRoles.Contains(KharbarchiRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new UserAdminOperationResult(false, "نمی‌توانید نقش مدیر کل را از حساب فعلی خودتان حذف کنید."));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removableRoles = currentRoles
            .Where(x => KharbarchiRoles.IsAssignableInternalRole(x) || string.Equals(x, KharbarchiRoles.LegacyAdmin, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (removableRoles.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, removableRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new UserAdminOperationResult(false, ToErrorText(removeResult)));
            }
        }

        var addResult = await _userManager.AddToRolesAsync(user, targetRoles);
        if (!addResult.Succeeded)
        {
            return BadRequest(new UserAdminOperationResult(false, ToErrorText(addResult)));
        }

        return Ok(new UserAdminOperationResult(true, "نقش‌ها با موفقیت ذخیره شد."));
    }

    [HttpPost("{userId}/password")]
    public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetLocalUserPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new UserAdminOperationResult(false, "کاربر پیدا نشد."));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new UserAdminOperationResult(false, ToErrorText(result)));
        }

        return Ok(new UserAdminOperationResult(true, "رمز عبور با موفقیت تغییر کرد."));
    }

    [HttpPost("{userId}/lock-state")]
    public async Task<IActionResult> SetLockState(string userId, [FromBody] SetLocalUserLockStateRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new UserAdminOperationResult(false, "کاربر پیدا نشد."));
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal) && request.IsLockedOut)
        {
            return BadRequest(new UserAdminOperationResult(false, "نمی‌توانید حساب فعلی خودتان را قفل کنید."));
        }

        var lockResult = request.IsLockedOut
            ? await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(50))
            : await _userManager.SetLockoutEndDateAsync(user, null);

        if (!lockResult.Succeeded)
        {
            return BadRequest(new UserAdminOperationResult(false, ToErrorText(lockResult)));
        }

        return Ok(new UserAdminOperationResult(true, request.IsLockedOut ? "حساب کاربر قفل شد." : "حساب کاربر فعال شد."));
    }

    private async Task<LocalUserDto> MapUserAsync(ApplicationUser user)
    {
        return new LocalUserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            user.FullName,
            await _userManager.IsLockedOutAsync(user),
            (await _userManager.GetRolesAsync(user)).OrderBy(x => x).ToList());
    }

    private async Task<RoleValidationResult> NormalizeAndValidateRolesAsync(IReadOnlyList<string> roles)
    {
        var normalized = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var role in normalized)
        {
            if (!KharbarchiRoles.IsAssignableInternalRole(role))
            {
                return new RoleValidationResult(false, [], $"نقش '{role}' مجاز نیست.");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return new RoleValidationResult(false, [], $"نقش '{role}' در دیتابیس وجود ندارد. برنامه را یک بار اجرا کنید تا Seed نقش‌ها انجام شود.");
            }
        }

        return new RoleValidationResult(true, normalized, string.Empty);
    }

    private sealed record RoleValidationResult(bool IsValid, IReadOnlyList<string> Roles, string Error);

    private static string ToErrorText(IdentityResult result)
    {
        return string.Join(" | ", result.Errors.Select(x => string.IsNullOrWhiteSpace(x.Description) ? x.Code : x.Description));
    }
}
