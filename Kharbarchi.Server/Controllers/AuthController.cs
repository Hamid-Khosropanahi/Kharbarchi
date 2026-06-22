using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth")]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwtOptions;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> jwtOptions,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        var allowPublicRegistration = _configuration.GetValue<bool>("Security:AllowPublicRegistration");
        if (!allowPublicRegistration)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new LoginResponse
            {
                IsSuccess = false,
                Message = "ثبت‌نام عمومی غیرفعال است. ساخت کاربر باید توسط ادمین انجام شود."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email?.Trim(),
            FullName = request.FullName?.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new LoginResponse
            {
                IsSuccess = false,
                Message = string.Join(" | ", result.Errors.Select(e => e.Description))
            });
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        return Ok(new LoginResponse
        {
            IsSuccess = true,
            Token = token,
            Message = "ثبت‌نام با موفقیت انجام شد."
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var normalizedUserName = request.UserName.Trim();
        var user = await _userManager.FindByNameAsync(normalizedUserName);

        if (user is null)
        {
            await Task.Delay(250);
            return Unauthorized(new LoginResponse { IsSuccess = false, Message = "نام کاربری یا رمز عبور اشتباه است." });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new LoginResponse { IsSuccess = false, Message = "حساب کاربری موقتاً قفل شده است." });
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordIsValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new LoginResponse { IsSuccess = false, Message = "نام کاربری یا رمز عبور اشتباه است." });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        return Ok(new LoginResponse
        {
            IsSuccess = true,
            Token = token,
            Message = "ورود موفقیت‌آمیز بود."
        });
    }

    private string GenerateJwt(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new Claim("full_name", user.FullName));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
