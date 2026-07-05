using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Services;

public sealed class BootstrapSuperAdminService
{
    private static readonly string[] RequiredAdminRoles =
    [
        KharbarchiRoles.SuperAdmin,
        KharbarchiRoles.LegacyAdmin
    ];

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly BootstrapSuperAdminOptions _options;
    private readonly ILogger<BootstrapSuperAdminService> _logger;

    public BootstrapSuperAdminService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<BootstrapSuperAdminOptions> options,
        ILogger<BootstrapSuperAdminService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var userName = _options.UserName!.Trim();
        var password = _options.Password!;

        await EnsureRequiredRolesAsync(cancellationToken);

        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = NormalizeOptionalValue(_options.Email),
                FullName = NormalizeOptionalValue(_options.DisplayName) ?? userName,
                EmailConfirmed = !string.IsNullOrWhiteSpace(_options.Email),
                LockoutEnabled = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            ThrowIfFailed(createResult, $"creating bootstrap SuperAdmin '{userName}'");
        }
        else
        {
            await UpdateSafeProfileFieldsAsync(user, cancellationToken);

            if (_options.ResetPasswordIfExists &&
                !await _userManager.CheckPasswordAsync(user, password))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, password);
                ThrowIfFailed(resetResult, $"resetting bootstrap SuperAdmin password for '{userName}'");
            }
        }

        foreach (var role in RequiredAdminRoles)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                continue;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            ThrowIfFailed(roleResult, $"assigning {role} role to '{userName}'");
        }

        _logger.LogInformation("Bootstrap SuperAdmin ensured. UserName={UserName}", userName);
    }

    private async Task EnsureRequiredRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var role in RequiredAdminRoles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await _roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            ThrowIfFailed(roleResult, $"creating role '{role}'");
        }
    }

    private async Task UpdateSafeProfileFieldsAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var changed = false;
        var configuredEmail = NormalizeOptionalValue(_options.Email);
        var configuredDisplayName = NormalizeOptionalValue(_options.DisplayName);

        if (configuredEmail is not null)
        {
            if (!string.Equals(user.Email, configuredEmail, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = configuredEmail;
                changed = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }
        }

        if (configuredDisplayName is not null &&
            !string.Equals(user.FullName, configuredDisplayName, StringComparison.Ordinal))
        {
            user.FullName = configuredDisplayName;
            changed = true;
        }

        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var updateResult = await _userManager.UpdateAsync(user);
        ThrowIfFailed(updateResult, $"updating bootstrap SuperAdmin '{user.UserName}'");
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ThrowIfFailed(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            " | ",
            result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException($"SuperAdmin bootstrap failed while {operation}. {errors}");
    }
}
