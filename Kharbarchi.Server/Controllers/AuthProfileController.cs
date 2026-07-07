using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthProfileController : ControllerBase
{
    private static readonly HashSet<string> HiddenClaimTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        JwtRegisteredClaimNames.Sub,
        JwtRegisteredClaimNames.Jti,
        JwtRegisteredClaimNames.Iat,
        JwtRegisteredClaimNames.Nbf,
        JwtRegisteredClaimNames.Exp,
        JwtRegisteredClaimNames.Iss,
        JwtRegisteredClaimNames.Aud,
        ClaimTypes.NameIdentifier,
        ClaimTypes.Role
    };

    private readonly UserManager<ApplicationUser> _userManager;

    public AuthProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserProfileDto>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = User.Claims
            .Where(IsSafeProfileClaim)
            .Select(claim => new SafeUserClaimDto(claim.Type, TrimClaimValue(claim.Value)))
            .Distinct()
            .OrderBy(claim => claim.Type, StringComparer.OrdinalIgnoreCase)
            .ThenBy(claim => claim.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new CurrentUserProfileDto
        {
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.FullName,
            Email = user.Email,
            Roles = roles.OrderBy(role => role, StringComparer.OrdinalIgnoreCase).ToList(),
            Claims = claims
        });
    }

    private static bool IsSafeProfileClaim(Claim claim)
    {
        if (HiddenClaimTypes.Contains(claim.Type)
            || string.IsNullOrWhiteSpace(claim.Value)
            || claim.Value.Length > 500)
        {
            return false;
        }

        var normalizedType = claim.Type.ToLowerInvariant();
        return !normalizedType.Contains("token", StringComparison.Ordinal)
            && !normalizedType.Contains("secret", StringComparison.Ordinal)
            && !normalizedType.Contains("password", StringComparison.Ordinal)
            && !normalizedType.Contains("authorization", StringComparison.Ordinal)
            && !normalizedType.Contains("consumer", StringComparison.Ordinal)
            && !normalizedType.Contains("key", StringComparison.Ordinal);
    }

    private static string TrimClaimValue(string value)
    {
        var text = value.Trim();
        return text.Length <= 200 ? text : text[..200] + "...";
    }
}
