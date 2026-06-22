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
    public ActionResult<IReadOnlyList<string>> GetRoles()
    {
        return Ok(KharbarchiRoles.AllSystemRoles.Where(x => x != KharbarchiRoles.Customer).OrderBy(x => x).ToList());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocalUserDto>>> GetUsers()
    {
        var users = _userManager.Users.OrderBy(x => x.UserName).Take(200).ToList();
        var result = new List<LocalUserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(new LocalUserDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email,
                user.FullName,
                await _userManager.IsLockedOutAsync(user),
                (await _userManager.GetRolesAsync(user)).OrderBy(x => x).ToList()));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LocalUserDto>> CreateUser([FromBody] CreateLocalUserRequest request)
    {
        var roles = NormalizeAndValidateRoles(request.Roles);

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email?.Trim(),
            FullName = request.FullName?.Trim(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(string.Join(" | ", createResult.Errors.Select(x => x.Description)));
        }

        foreach (var role in roles)
        {
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                return BadRequest(string.Join(" | ", addResult.Errors.Select(x => x.Description)));
            }
        }

        return CreatedAtAction(nameof(GetUsers), routeValues: null, value: new LocalUserDto(user.Id, user.UserName ?? string.Empty, user.Email, user.FullName, false, roles));
    }

    [HttpPut("{userId}/roles")]
    public async Task<IActionResult> UpdateRoles(string userId, [FromBody] UpdateLocalUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var roles = NormalizeAndValidateRoles(request.Roles);
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return BadRequest(string.Join(" | ", removeResult.Errors.Select(x => x.Description)));
        }

        foreach (var role in roles)
        {
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                return BadRequest(string.Join(" | ", addResult.Errors.Select(x => x.Description)));
            }
        }

        return NoContent();
    }

    private IReadOnlyList<string> NormalizeAndValidateRoles(IReadOnlyList<string> roles)
    {
        var normalized = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var role in normalized)
        {
            if (!KharbarchiRoles.AllSystemRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Role '{role}' is not allowed.");
            }
        }

        return normalized;
    }
}
