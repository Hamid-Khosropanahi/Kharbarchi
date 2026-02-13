using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "User");
        return Ok();
    }
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // 1. Check if user exists
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            // Return 200 OK with IsSuccess=false so client can read the JSON body easily
            // Or return 400/401, but consistent JSON structure is easier for Blazor clients
            return Ok(new LoginResponse { IsSuccess = false, Message = "کاربری با این مشخصات یافت نشد." });
        }

        // 2. Check Password
        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
        {
            return Ok(new LoginResponse { IsSuccess = false, Message = "رمز عبور اشتباه است." });
        }

        // 3. Get Roles (Admin, Customer_Full_Access, etc.)
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Generate Token with Claims
        var token = GenerateJwt(user, roles);

        return Ok(new LoginResponse { IsSuccess = true, Token = token, Message = "ورود موفقیت آمیز بود." });
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var key = _config["Jwt:Key"]!;
        var issuer = _config["Jwt:Issuer"]!;

        // Standard Claims
        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.UserName!),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // Add Roles as Claims (This handles Admin, Customer_New, Customer_Full_Access, etc.)
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // If you have specific custom claims in the database distinct from roles:
        // var userClaims = await _userManager.GetClaimsAsync(user);
        // claims.AddRange(userClaims);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}