using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kharbarchi.Server.Options;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Services;

public sealed class BarookCpgClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly BarookOptions _options;
    private readonly ILogger<BarookCpgClient> _logger;

    public BarookCpgClient(HttpClient httpClient, IOptions<BarookOptions> options, ILogger<BarookCpgClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BarookStartPaymentClientResult> StartPaymentAsync(BarookStartPaymentClientRequest request, CancellationToken cancellationToken)
    {
        var payload = request with
        {
            TerminalCode = _options.CpgTerminalCode,
            Password = _options.CpgPassword
        };

        using var response = await _httpClient.PostAsJsonAsync(TrimSlash(_options.StartPaymentPath), payload, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Barook start-payment failed. Status={Status}, Body={Body}", (int)response.StatusCode, TrimForLog(content));
            throw new HttpRequestException($"Barook start-payment failed: {(int)response.StatusCode}. Body: {TrimForLog(content)}", null, response.StatusCode);
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
        var token = GetString(document.RootElement, "token") ?? GetString(document.RootElement, "Token");
        var expireDate = GetDateTime(document.RootElement, "expireDate") ?? GetDateTime(document.RootElement, "ExpireDate");

        return new BarookStartPaymentClientResult(token, expireDate, content);
    }

    public async Task<BarookVerifyPaymentClientResult> VerifyPaymentAsync(string externalCode, string token, CancellationToken cancellationToken)
    {
        var payload = new BarookVerifyPaymentClientRequest(
            _options.CpgTerminalCode,
            _options.CpgPassword,
            externalCode,
            token);

        using var response = await _httpClient.PostAsJsonAsync(TrimSlash(_options.VerifyPaymentPath), payload, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Barook verify-payment failed. Status={Status}, Body={Body}", (int)response.StatusCode, TrimForLog(content));
            throw new HttpRequestException($"Barook verify-payment failed: {(int)response.StatusCode}. Body: {TrimForLog(content)}", null, response.StatusCode);
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
        var root = document.RootElement;

        return new BarookVerifyPaymentClientResult(
            Status: GetString(root, "status") ?? GetString(root, "Status"),
            ReferenceNumber: GetString(root, "referenceNumber") ?? GetString(root, "ReferenceNumber") ?? GetString(root, "code"),
            CardNumber: GetString(root, "cardNumber") ?? GetString(root, "CardNumber"),
            TotalAmount: GetDecimal(root, "totalAmount") ?? GetDecimal(root, "TotalAmount"),
            TransactionId: GetString(root, "referenceNumber") ?? GetString(root, "code") ?? externalCode,
            RawJson: content);
    }

    public string BuildRedirectUrl(string token)
    {
        var relative = _options.RedirectPathTemplate.Replace("{token}", Uri.EscapeDataString(token), StringComparison.OrdinalIgnoreCase);
        return _options.CpgBaseUrl.TrimEnd('/') + "/" + relative.TrimStart('/');
    }

    private static string TrimSlash(string path) => path.TrimStart('/');

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.ToString()
            : null;
    }

    private static DateTime? GetDateTime(JsonElement element, string propertyName)
    {
        var value = GetString(element, propertyName);
        return DateTime.TryParse(value, out var date) ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : null;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var numeric))
        {
            return numeric;
        }

        return decimal.TryParse(property.ToString(), out var parsed) ? parsed : null;
    }

    private static string TrimForLog(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 1000 ? value : value[..1000] + "...";
    }
}

public sealed record BarookSaleItemClientDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("saleCount")] decimal SaleCount,
    [property: JsonPropertyName("unitType")] string UnitType,
    [property: JsonPropertyName("amount")] decimal Amount);

public sealed record BarookStartPaymentClientRequest(
    [property: JsonPropertyName("terminalCode")] string TerminalCode,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("externalCode")] string ExternalCode,
    [property: JsonPropertyName("paymentMonthCount")] int? PaymentMonthCount,
    [property: JsonPropertyName("paymentDayCount")] int? PaymentDayCount,
    [property: JsonPropertyName("ownerName")] string OwnerName,
    [property: JsonPropertyName("ownerMobile")] string? OwnerMobile,
    [property: JsonPropertyName("ownerNationalCode")] string OwnerNationalCode,
    [property: JsonPropertyName("redirectUrl")] string RedirectUrl,
    [property: JsonPropertyName("saleItemDto")] IReadOnlyList<BarookSaleItemClientDto> SaleItemDto,
    [property: JsonPropertyName("branchCode")] string? BranchCode,
    [property: JsonPropertyName("businessServiceSlug")] string? BusinessServiceSlug,
    [property: JsonPropertyName("attributes")] IReadOnlyDictionary<string, string>? Attributes);

public sealed record BarookVerifyPaymentClientRequest(
    [property: JsonPropertyName("terminalCode")] string TerminalCode,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("externalCode")] string ExternalCode,
    [property: JsonPropertyName("token")] string Token);

public sealed record BarookStartPaymentClientResult(string? Token, DateTime? ExpireDateUtc, string RawJson);

public sealed record BarookVerifyPaymentClientResult(
    string? Status,
    string? ReferenceNumber,
    string? CardNumber,
    decimal? TotalAmount,
    string? TransactionId,
    string RawJson);
