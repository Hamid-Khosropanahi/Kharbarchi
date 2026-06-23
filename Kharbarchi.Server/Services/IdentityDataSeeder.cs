using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Services;

public sealed class IdentityDataSeeder
{
    public const string DevelopmentTestPassword = "Kharbarchi@Test123!";

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedAdminOptions _seedAdminOptions;
    private readonly GatewayOptions _gatewayOptions;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SeedAdminOptions> seedAdminOptions,
        IOptions<GatewayOptions> gatewayOptions,
        IHostEnvironment environment,
        ILogger<IdentityDataSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seedAdminOptions = seedAdminOptions.Value;
        _gatewayOptions = gatewayOptions.Value;
        _environment = environment;
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

        if (_environment.IsDevelopment())
        {
            await SeedDevelopmentUsersAsync(cancellationToken);
        }
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
        var adminUser = await EnsureUserAsync(
            _seedAdminOptions.UserName,
            _seedAdminOptions.Email,
            _seedAdminOptions.FullName,
            _seedAdminOptions.Password,
            resetPasswordWhenExists: false,
            cancellationToken);

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
        var user = await EnsureUserAsync(
            _gatewayOptions.AllowedUserName,
            _gatewayOptions.Email,
            _gatewayOptions.FullName,
            _gatewayOptions.Password,
            resetPasswordWhenExists: false,
            cancellationToken);

        await EnsureUserRoleAsync(user, KharbarchiRoles.SuperAdmin);
        await EnsureUserRoleAsync(user, _gatewayOptions.RequiredGatewayRole);
    }

    private async Task SeedDevelopmentUsersAsync(CancellationToken cancellationToken)
    {
        var users = new[]
        {
            new SeedUser("superadmin", "superadmin@kharbarchi.local", "مدیر کل تست", new[] { KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin }),
            new SeedUser("pricing.manager", "pricing.manager@kharbarchi.local", "مدیر قیمت‌گذاری تست", new[] { KharbarchiRoles.PricingManager }),
            new SeedUser("pricing.employee", "pricing.employee@kharbarchi.local", "کارشناس قیمت‌گذاری تست", new[] { KharbarchiRoles.PricingEmployee }),
            new SeedUser("warehouse", "warehouse@kharbarchi.local", "انباردار تست", new[] { KharbarchiRoles.WarehouseEmployee }),
            new SeedUser("sales.manager", "sales.manager@kharbarchi.local", "مدیر فروش تست", new[] { KharbarchiRoles.SalesManager }),
            new SeedUser("shipping.manager", "shipping.manager@kharbarchi.local", "مسئول ارسال تست", new[] { KharbarchiRoles.ShippingOrderManager }),
            new SeedUser("accountant", "accountant@kharbarchi.local", "حسابدار تست", new[] { KharbarchiRoles.Accountant }),
            new SeedUser("sync.agent", "sync.agent@kharbarchi.local", "عامل همگام‌سازی تست", new[] { KharbarchiRoles.CentralSyncAgent }),
            new SeedUser("gateway.admin", "gateway.admin@kharbarchi.local", "کاربر درگاه تست", new[] { KharbarchiRoles.GatewayAdmin }),
            new SeedUser("customer.test", "customer.test@kharbarchi.local", "مشتری تست", new[] { KharbarchiRoles.Customer })
        };

        foreach (var seedUser in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await EnsureUserAsync(
                seedUser.UserName,
                seedUser.Email,
                seedUser.FullName,
                DevelopmentTestPassword,
                resetPasswordWhenExists: true,
                cancellationToken);

            await ReplaceRolesAsync(user, seedUser.Roles);
        }

        _logger.LogInformation("Development test users were seeded. Password for all test users is {Password}. Do not enable this outside Development.", DevelopmentTestPassword);
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        string userName,
        string? email,
        string? fullName,
        string password,
        bool resetPasswordWhenExists,
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

        if (resetPasswordWhenExists && !await _userManager.CheckPasswordAsync(user, password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, password);
            ThrowIfFailed(resetResult, $"resetting development seed password for '{normalizedUserName}'");
        }

        return user;
    }

    private async Task ReplaceRolesAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        var targetRoles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeRoles = currentRoles.Except(targetRoles, StringComparer.OrdinalIgnoreCase).ToArray();
        var addRoles = targetRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();

        if (removeRoles.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, removeRoles);
            ThrowIfFailed(removeResult, $"removing stale roles from {user.UserName}");
        }

        foreach (var role in addRoles)
        {
            await EnsureUserRoleAsync(user, role);
        }
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

    private sealed record SeedUser(string UserName, string Email, string FullName, IReadOnlyList<string> Roles);
}
