namespace Kharbarchi.Shared.Contracts.WooCommerce;

public sealed class WooConnectionProfileDto
{
    public int Id { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string EnvironmentType { get; set; } = "Local";
    public string BaseUrl { get; set; } = string.Empty;
    public string ConsumerKeyMasked { get; set; } = string.Empty;
    public bool HasConsumerSecret { get; set; }
    public string ApiVersion { get; set; } = "wc/v3";
    public bool VerifySsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public bool IsActive { get; set; }
    public DateTime? LastTestedAtUtc { get; set; }
    public bool? LastTestSucceeded { get; set; }
    public string? LastTestMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class WooConnectionProfileUpsertRequest
{
    public int? Id { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string EnvironmentType { get; set; } = "Local";
    public string BaseUrl { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string? ConsumerSecret { get; set; }
    public string ApiVersion { get; set; } = "wc/v3";
    public bool VerifySsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public bool IsActive { get; set; }
}

public sealed class WooConnectionProfileTestResultDto
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
    public string? ResponsePreview { get; set; }
    public DateTime TestedAtUtc { get; set; }
}
