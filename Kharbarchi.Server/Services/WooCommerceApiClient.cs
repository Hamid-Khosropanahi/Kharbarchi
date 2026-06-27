using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Kharbarchi.Server.Contracts;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Options;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Services;

public sealed class WooCommerceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly HttpClient _httpClient;
    private readonly WooCommerceOptions _options;
    private readonly ILogger<WooCommerceApiClient> _logger;

    public WooCommerceApiClient(HttpClient httpClient, IOptions<WooCommerceOptions> options, ILogger<WooCommerceApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        ConfigureAuthenticationHeader();
    }

    public Task<JsonDocument> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return SendForJsonDocumentAsync(HttpMethod.Get, $"wp-json/wc/v3/products?page={page}&per_page={pageSize}&orderby=id&order=asc&status=any", null, cancellationToken);
    }

    public Task<JsonDocument> GetProductVariationsAsync(long productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return SendForJsonDocumentAsync(HttpMethod.Get, $"wp-json/wc/v3/products/{productId}/variations?page={page}&per_page={pageSize}&orderby=id&order=asc", null, cancellationToken);
    }

    public async Task<JsonDocument> GetOrdersAsync(string? status, DateTime? afterUtc, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = new List<string>
        {
            $"page={page}",
            $"per_page={pageSize}",
            "orderby=date",
            "order=desc"
        };

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "any", StringComparison.OrdinalIgnoreCase))
        {
            query.Add($"status={Uri.EscapeDataString(status.Trim())}");
        }

        if (afterUtc.HasValue)
        {
            query.Add($"after={Uri.EscapeDataString(afterUtc.Value.ToUniversalTime().ToString("O"))}");
        }

        return await SendForJsonDocumentAsync(HttpMethod.Get, $"wp-json/wc/v3/orders?{string.Join('&', query)}", null, cancellationToken);
    }

    public Task<JsonDocument> GetOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Get, $"wp-json/wc/v3/orders/{orderId}", null, cancellationToken);
    }

    public Task<JsonDocument> UpdateProductAsync(long productId, WooCommerceProductUpdatePayload payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Put, $"wp-json/wc/v3/products/{productId}", payload, cancellationToken);
    }

    public Task<JsonDocument> UpdateVariationAsync(long productId, long variationId, WooCommerceProductUpdatePayload payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Put, $"wp-json/wc/v3/products/{productId}/variations/{variationId}", payload, cancellationToken);
    }

    public Task<JsonDocument> MarkOrderPaymentAsync(long orderId, WooCommercePaymentUpdatePayload payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Put, $"wp-json/wc/v3/orders/{orderId}", payload, cancellationToken);
    }

    public Task<JsonDocument> UpdateOrderAsync(long orderId, object payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Put, $"wp-json/wc/v3/orders/{orderId}", payload, cancellationToken);
    }

    public Task<JsonDocument> AddOrderNoteAsync(long orderId, WooCommerceOrderNotePayload payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Post, $"wp-json/wc/v3/orders/{orderId}/notes", payload, cancellationToken);
    }


    public Task<JsonDocument> CreateProductAsync(WooProductUpsertPayload payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Post, "wp-json/wc/v3/products", payload, cancellationToken);
    }

    public Task<JsonDocument> UpdateProductAsync(long productId, WooProductUpsertPayload payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Put, $"wp-json/wc/v3/products/{productId}", payload, cancellationToken);
    }

    public Task<JsonDocument> GetProductCategoriesBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Get, $"wp-json/wc/v3/products/categories?slug={Uri.EscapeDataString(slug)}&per_page=100", null, cancellationToken);
    }

    public Task<JsonDocument> CreateProductCategoryAsync(object payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Post, "wp-json/wc/v3/products/categories", payload, cancellationToken);
    }

    public Task<JsonDocument> UpdateProductCategoryAsync(long categoryId, object payload, CancellationToken cancellationToken)
    {
        return SendForJsonDocumentAsync(HttpMethod.Put, $"wp-json/wc/v3/products/categories/{categoryId}", payload, cancellationToken);
    }

    public Task<JsonDocument> PostWordPressEndpointAsync(string relativeUrl, object payload, CancellationToken cancellationToken)
    {
        var url = relativeUrl.TrimStart('/');
        return SendForJsonDocumentAsync(HttpMethod.Post, url, payload, cancellationToken);
    }

    private async Task<JsonDocument> SendForJsonDocumentAsync(HttpMethod method, string relativeUrl, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, relativeUrl);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("WooCommerce API failed. Method={Method}, Url={Url}, Status={Status}, Body={Body}",
                method, relativeUrl, (int)response.StatusCode, TrimForLog(content));

            throw new HttpRequestException(
                $"WooCommerce API returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {TrimForLog(content)}",
                null,
                response.StatusCode);
        }

        return JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
    }

    private void ConfigureAuthenticationHeader()
    {
        var raw = $"{_options.ConsumerKey}:{_options.ConsumerSecret}";
        var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(raw));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
