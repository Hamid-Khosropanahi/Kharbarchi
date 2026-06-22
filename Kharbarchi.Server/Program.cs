using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key is required.")
    .Validate(o => Encoding.UTF8.GetByteCount(o.Key) >= 32, "Jwt:Key must be at least 32 bytes.")
    .Validate(o => o.ExpirationMinutes is >= 15 and <= 1440, "Jwt:ExpirationMinutes must be between 15 and 1440.")
    .ValidateOnStart();

builder.Services.AddOptions<SeedAdminOptions>()
    .Bind(builder.Configuration.GetSection(SeedAdminOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddOptions<WooCommerceOptions>()
    .Bind(builder.Configuration.GetSection(WooCommerceOptions.SectionName))
    .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps,
        "WooCommerce:BaseUrl must be an absolute HTTPS URL.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConsumerKey), "WooCommerce:ConsumerKey is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConsumerSecret), "WooCommerce:ConsumerSecret is required.")
    .ValidateOnStart();

builder.Services.AddOptions<GatewayOptions>()
    .Bind(builder.Configuration.GetSection(GatewayOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.AllowedUserName), "Gateway:AllowedUserName is required.")
    .ValidateOnStart();

builder.Services.AddOptions<BarookOptions>()
    .Bind(builder.Configuration.GetSection(BarookOptions.SectionName))
    .Validate(o => Uri.TryCreate(o.CpgBaseUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps,
        "Barook:CpgBaseUrl must be an absolute HTTPS URL.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.CpgTerminalCode), "Barook:CpgTerminalCode is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.CpgPassword), "Barook:CpgPassword is required.")
    .Validate(o => o.DefaultPaymentDayCount is >= 1 and <= 180, "Barook:DefaultPaymentDayCount must be between 1 and 180.")
    .Validate(o => o.AmountMultiplierToRial > 0, "Barook:AmountMultiplierToRial must be greater than zero.")
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured. Use User Secrets or Environment Variables.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt options are not configured.");

var gatewayOptions = builder.Configuration.GetSection(GatewayOptions.SectionName).Get<GatewayOptions>()
    ?? throw new InvalidOperationException("Gateway options are not configured.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(AuthorizationPolicyNames.AdminOnly, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin))
    .AddPolicy(AuthorizationPolicyNames.SuperAdminOnly, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin))
    .AddPolicy(AuthorizationPolicyNames.CatalogRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.PricingEmployee, KharbarchiRoles.WarehouseEmployee, KharbarchiRoles.SalesManager))
    .AddPolicy(AuthorizationPolicyNames.CatalogWrite, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager))
    .AddPolicy(AuthorizationPolicyNames.PriceRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.PricingEmployee, KharbarchiRoles.SalesManager))
    .AddPolicy(AuthorizationPolicyNames.PurchasePriceRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager))
    .AddPolicy(AuthorizationPolicyNames.PriceProposalCreate, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.PricingEmployee))
    .AddPolicy(AuthorizationPolicyNames.PriceProposalManagerApproval, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager))
    .AddPolicy(AuthorizationPolicyNames.PriceProposalFinalApproval, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin))
    .AddPolicy(AuthorizationPolicyNames.StockRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.WarehouseEmployee, KharbarchiRoles.SalesManager))
    .AddPolicy(AuthorizationPolicyNames.InventoryProposalCreate, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.WarehouseEmployee))
    .AddPolicy(AuthorizationPolicyNames.InventoryProposalManagerApproval, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager))
    .AddPolicy(AuthorizationPolicyNames.InventoryProposalFinalApproval, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin))
    .AddPolicy(AuthorizationPolicyNames.ProductImportRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.SalesManager))
    .AddPolicy(AuthorizationPolicyNames.ProductImportWrite, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager))
    .AddPolicy(AuthorizationPolicyNames.OrdersRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.PricingManager, KharbarchiRoles.SalesManager, KharbarchiRoles.ShippingOrderManager, KharbarchiRoles.Accountant))
    .AddPolicy(AuthorizationPolicyNames.OrdersImportWrite, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.SalesManager, KharbarchiRoles.ShippingOrderManager))
    .AddPolicy(AuthorizationPolicyNames.OrderPaymentWorkflow, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.ShippingOrderManager, KharbarchiRoles.Accountant))
    .AddPolicy(AuthorizationPolicyNames.BarookPaymentOperator, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.ShippingOrderManager))
    .AddPolicy(AuthorizationPolicyNames.AccountingOrdersRead, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.Accountant))
    .AddPolicy(AuthorizationPolicyNames.ManualReceiptCreate, policy =>
        policy.RequireRole(KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin, KharbarchiRoles.Accountant))
    .AddPolicy(AuthorizationPolicyNames.CentralSyncAgentOnly, policy =>
        policy.RequireRole(KharbarchiRoles.CentralSyncAgent, KharbarchiRoles.SuperAdmin, KharbarchiRoles.LegacyAdmin))
    .AddPolicy(AuthorizationPolicyNames.GatewayAdminOnly, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var userName = context.User.Identity?.Name;
            var hasRoles = (context.User.IsInRole(KharbarchiRoles.SuperAdmin) || context.User.IsInRole(KharbarchiRoles.LegacyAdmin) || context.User.IsInRole(gatewayOptions.RequiredAdminRole)) &&
                           context.User.IsInRole(gatewayOptions.RequiredGatewayRole);

            return hasRoles && (!gatewayOptions.EnforceAllowedUserName ||
                   string.Equals(userName, gatewayOptions.AllowedUserName, StringComparison.OrdinalIgnoreCase));
        });
    });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustedClients", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.DisallowCredentials();
            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("admin", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("payment", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("gateway", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddScoped<IdentityDataSeeder>();
builder.Services.AddScoped<WooCommerceSyncService>();
builder.Services.AddScoped<SyncOutboxService>();
builder.Services.AddScoped<WooCommerceImportService>();
builder.Services.AddScoped<BarookPaymentService>();
builder.Services.AddScoped<AccountingReceiptService>();

builder.Services.AddHttpClient<WooCommerceApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WooCommerceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Kharbarchi-Support-Api/1.0");
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var environment = sp.GetRequiredService<IHostEnvironment>();
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WooCommerceOptions>>().Value;
    var handler = new HttpClientHandler();

    if (environment.IsDevelopment() && options.AllowInsecureLocalhostSsl)
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

builder.Services.AddHttpClient<BarookCpgClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BarookOptions>>().Value;
    client.BaseAddress = new Uri(options.CpgBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Kharbarchi-Barook-Cpg/1.0");
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var environment = sp.GetRequiredService<IHostEnvironment>();
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BarookOptions>>().Value;
    var handler = new HttpClientHandler();

    if (environment.IsDevelopment() && options.AllowInsecureLocalhostSsl)
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kharbarchi Local Support API",
        Version = "v1",
        Description = "Secure local support layer for WooCommerce products, orders, Barook payment workflow and accounting receipts."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'");
    await next();
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors("TrustedClients");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
