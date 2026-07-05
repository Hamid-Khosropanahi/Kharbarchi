using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Services;

public sealed class IdentityDataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GatewayOptions _gatewayOptions;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<GatewayOptions> gatewayOptions,
        ILogger<IdentityDataSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _gatewayOptions = gatewayOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var role in KharbarchiRoles.AllSystemRoles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                ThrowIfFailed(roleResult, $"creating role '{role}'");
            }
        }

        await SeedGatewayUserAsync(cancellationToken);
    }

    private async Task SeedGatewayUserAsync(CancellationToken cancellationToken)
    {
        if (!_gatewayOptions.SeedGatewayUser)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_gatewayOptions.Password))
        {
            _logger.LogWarning("Gateway:SeedGatewayUser is enabled, but no password was configured. Gateway user was not created.");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var user = await EnsureUserAsync(
            _gatewayOptions.AllowedUserName,
            _gatewayOptions.Email,
            _gatewayOptions.FullName,
            _gatewayOptions.Password,
            cancellationToken);

        await EnsureUserRoleAsync(user, KharbarchiRoles.SuperAdmin);
        await EnsureUserRoleAsync(user, _gatewayOptions.RequiredGatewayRole);
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        string userName,
        string? email,
        string? fullName,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedUserName = userName.Trim();
        var user = await _userManager.FindByNameAsync(normalizedUserName);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedUserName,
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                FullName = string.IsNullOrWhiteSpace(fullName) ? normalizedUserName : fullName.Trim(),
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            ThrowIfFailed(createResult, $"creating seed user '{normalizedUserName}'");
            return user;
        }

        var changed = false;
        var safeFullName = string.IsNullOrWhiteSpace(fullName) ? normalizedUserName : fullName.Trim();
        var safeEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        if (!string.Equals(user.FullName, safeFullName, StringComparison.Ordinal))
        {
            user.FullName = safeFullName;
            changed = true;
        }

        if (!string.Equals(user.Email, safeEmail, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = safeEmail;
            changed = true;
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            changed = true;
        }

        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;
            changed = true;
        }

        if (changed)
        {
            var updateResult = await _userManager.UpdateAsync(user);
            ThrowIfFailed(updateResult, $"updating seed user '{normalizedUserName}'");
        }

        return user;
    }

    private async Task EnsureUserRoleAsync(ApplicationUser user, string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            ThrowIfFailed(roleResult, $"creating role '{role}'");
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            ThrowIfFailed(addRoleResult, $"assigning {role} role to {user.UserName}");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        throw new InvalidOperationException($"Identity seed failed while {operation}. {errors}");
    }
}
