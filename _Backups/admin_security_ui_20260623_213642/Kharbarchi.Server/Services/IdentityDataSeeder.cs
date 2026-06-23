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
    private readonly SeedAdminOptions _seedAdminOptions;
    private readonly GatewayOptions _gatewayOptions;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SeedAdminOptions> seedAdminOptions,
        IOptions<GatewayOptions> gatewayOptions,
        ILogger<IdentityDataSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seedAdminOptions = seedAdminOptions.Value;
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

        await SeedAdminUserAsync(cancellationToken);
        await SeedGatewayUserAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        if (!_seedAdminOptions.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_seedAdminOptions.Password))
        {
            _logger.LogWarning("SeedAdmin is enabled, but no password was configured. Admin user was not created.");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var adminUser = await _userManager.FindByNameAsync(_seedAdminOptions.UserName);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = _seedAdminOptions.UserName,
                Email = _seedAdminOptions.Email,
                FullName = _seedAdminOptions.FullName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(adminUser, _seedAdminOptions.Password);
            ThrowIfFailed(createResult, "creating seed super admin user");
        }

        await EnsureUserRoleAsync(adminUser, KharbarchiRoles.SuperAdmin);
        await EnsureUserRoleAsync(adminUser, KharbarchiRoles.LegacyAdmin);
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
        var user = await _userManager.FindByNameAsync(_gatewayOptions.AllowedUserName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = _gatewayOptions.AllowedUserName,
                Email = _gatewayOptions.Email,
                FullName = _gatewayOptions.FullName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, _gatewayOptions.Password);
            ThrowIfFailed(createResult, "creating gateway integration user");
        }

        await EnsureUserRoleAsync(user, KharbarchiRoles.SuperAdmin);
        await EnsureUserRoleAsync(user, _gatewayOptions.RequiredGatewayRole);
    }

    private async Task EnsureUserRoleAsync(ApplicationUser user, string role)
    {
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
