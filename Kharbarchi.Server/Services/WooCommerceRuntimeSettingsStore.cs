using System.Text.Json;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Contracts.WooCommerce;

namespace Kharbarchi.Server.Services;

public sealed class WooCommerceRuntimeSettingsStore
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<WooCommerceRuntimeSettingsStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public WooCommerceRuntimeSettingsStore(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<WooCommerceRuntimeSettingsStore> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    private string SettingsDirectory => Path.Combine(_environment.ContentRootPath, "App_Data");
    private string SettingsPath => Path.Combine(SettingsDirectory, "woocommerce.runtime.local.json");

    public async Task<WooCommerceRuntimeSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        WooCommerceRuntimeSettings settings = LoadFromConfiguration();

        if (!File.Exists(SettingsPath))
        {
            return Normalize(settings);
        }

        try
        {
            await using var stream = File.OpenRead(SettingsPath);
            var saved = await JsonSerializer.DeserializeAsync<WooCommerceRuntimeSettings>(stream, _jsonOptions, cancellationToken);
            if (saved is null)
            {
                return Normalize(settings);
            }

            // IMPORTANT: Do NOT load raw secrets (ConsumerSecret) from disk. Existing files may contain
            // raw secrets from older versions; ignore those values for runtime safety. Do not log them.
            return Normalize(new WooCommerceRuntimeSettings
            {
                BaseUrl = string.IsNullOrWhiteSpace(saved.BaseUrl) ? settings.BaseUrl : saved.BaseUrl,
                ConsumerKey = string.IsNullOrWhiteSpace(saved.ConsumerKey) ? settings.ConsumerKey : saved.ConsumerKey,
                // Do NOT populate ConsumerSecret from saved file. Keep secret empty in runtime settings unless
                // explicitly provided via API in this runtime (which will not be persisted raw to disk).
                ConsumerSecret = string.Empty,
                TimeoutSeconds = saved.TimeoutSeconds <= 0 ? settings.TimeoutSeconds : saved.TimeoutSeconds,
                AllowInsecureLocalhostSsl = saved.AllowInsecureLocalhostSsl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot read WooCommerce runtime settings from {SettingsPath}", SettingsPath);
            return Normalize(settings);
        }
    }

    public async Task<WooCommerceRuntimeSettings> SaveAsync(WooConnectionSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var current = await LoadAsync(cancellationToken);
        var settings = Normalize(new WooCommerceRuntimeSettings
        {
            BaseUrl = dto.BaseUrl,
            ConsumerKey = dto.ConsumerKey,
            // Keep consumer secret in-memory for runtime operations, but do NOT persist raw secrets to disk.
            // If dto.ConsumerSecret is blank, preserve current in-memory secret; however we will not write it to disk.
            ConsumerSecret = string.IsNullOrWhiteSpace(dto.ConsumerSecret) ? current.ConsumerSecret : dto.ConsumerSecret,
            TimeoutSeconds = dto.TimeoutSeconds,
            AllowInsecureLocalhostSsl = dto.AllowInsecureLocalhostSsl
        });

        Directory.CreateDirectory(SettingsDirectory);
        // When persisting runtime settings, never write raw secrets to disk.
        var persisted = new WooCommerceRuntimeSettings
        {
            BaseUrl = settings.BaseUrl,
            ConsumerKey = settings.ConsumerKey,
            ConsumerSecret = string.Empty,
            TimeoutSeconds = settings.TimeoutSeconds,
            AllowInsecureLocalhostSsl = settings.AllowInsecureLocalhostSsl
        };

        await using var stream = File.Create(SettingsPath);
        await JsonSerializer.SerializeAsync(stream, persisted, _jsonOptions, cancellationToken);
        return settings;
    }

    public WooConnectionSettingsDto ToDto(WooCommerceRuntimeSettings settings, bool includeSecret = false)
    {
        return new WooConnectionSettingsDto
        {
            BaseUrl = settings.BaseUrl,
            ConsumerKey = settings.ConsumerKey,
            ConsumerSecret = includeSecret ? settings.ConsumerSecret : string.Empty,
            HasConsumerSecret = !string.IsNullOrWhiteSpace(settings.ConsumerSecret),
            TimeoutSeconds = settings.TimeoutSeconds,
            AllowInsecureLocalhostSsl = settings.AllowInsecureLocalhostSsl
        };
    }

    public async Task<WooCommerceRuntimeSettings> MergeAsync(WooConnectionTestRequest request, CancellationToken cancellationToken = default)
    {
        var saved = await LoadAsync(cancellationToken);
        return Normalize(new WooCommerceRuntimeSettings
        {
            BaseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? saved.BaseUrl : request.BaseUrl!,
            ConsumerKey = string.IsNullOrWhiteSpace(request.ConsumerKey) ? saved.ConsumerKey : request.ConsumerKey!,
            ConsumerSecret = string.IsNullOrWhiteSpace(request.ConsumerSecret) ? saved.ConsumerSecret : request.ConsumerSecret!,
            TimeoutSeconds = request.TimeoutSeconds.GetValueOrDefault(saved.TimeoutSeconds),
            AllowInsecureLocalhostSsl = request.AllowInsecureLocalhostSsl.GetValueOrDefault(saved.AllowInsecureLocalhostSsl)
        });
    }

    public async Task<WooCommerceRuntimeSettings> MergeAsync(WooRawApiRequest request, CancellationToken cancellationToken = default)
    {
        var saved = await LoadAsync(cancellationToken);
        return Normalize(new WooCommerceRuntimeSettings
        {
            BaseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? saved.BaseUrl : request.BaseUrl!,
            ConsumerKey = string.IsNullOrWhiteSpace(request.ConsumerKey) ? saved.ConsumerKey : request.ConsumerKey!,
            ConsumerSecret = string.IsNullOrWhiteSpace(request.ConsumerSecret) ? saved.ConsumerSecret : request.ConsumerSecret!,
            TimeoutSeconds = request.TimeoutSeconds.GetValueOrDefault(saved.TimeoutSeconds),
            AllowInsecureLocalhostSsl = request.AllowInsecureLocalhostSsl.GetValueOrDefault(saved.AllowInsecureLocalhostSsl)
        });
    }

    private WooCommerceRuntimeSettings LoadFromConfiguration()
    {
        var section = _configuration.GetSection("WooCommerce");
        return Normalize(new WooCommerceRuntimeSettings
        {
            BaseUrl = section["BaseUrl"] ?? string.Empty,
            ConsumerKey = section["ConsumerKey"] ?? string.Empty,
            ConsumerSecret = section["ConsumerSecret"] ?? string.Empty,
            TimeoutSeconds = int.TryParse(section["TimeoutSeconds"], out var timeout) ? timeout : 30,
            AllowInsecureLocalhostSsl = bool.TryParse(section["AllowInsecureLocalhostSsl"], out var allow) ? allow : _environment.IsDevelopment()
        });
    }

    private static WooCommerceRuntimeSettings Normalize(WooCommerceRuntimeSettings settings)
    {
        settings.BaseUrl = (settings.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        settings.ConsumerKey = (settings.ConsumerKey ?? string.Empty).Trim();
        settings.ConsumerSecret = (settings.ConsumerSecret ?? string.Empty).Trim();
        settings.TimeoutSeconds = settings.TimeoutSeconds is < 5 or > 180 ? 30 : settings.TimeoutSeconds;
        return settings;
    }
}
