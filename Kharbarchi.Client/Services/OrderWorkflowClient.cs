using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;

namespace Kharbarchi.Client.Services;

public sealed class OrderWorkflowClient
{
    private readonly HttpClient _http;

    public OrderWorkflowClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ImportResult?> ImportProductsAsync(ImportWooCommerceProductsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/admin/woocommerce-import/products", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImportResult>(cancellationToken: cancellationToken);
    }

    public async Task<ImportResult?> ImportOrdersAsync(ImportWooCommerceOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/admin/woocommerce-import/orders", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImportResult>(cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<LocalWooOrderListItemDto>?> GetOrdersAsync(string? internalStatus = null, string? paymentStatus = null, string? search = null, int page = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/local-orders?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(internalStatus)) query += $"&internalStatus={Uri.EscapeDataString(internalStatus)}";
        if (!string.IsNullOrWhiteSpace(paymentStatus)) query += $"&paymentStatus={Uri.EscapeDataString(paymentStatus)}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        return _http.GetFromJsonAsync<IReadOnlyList<LocalWooOrderListItemDto>>(query, cancellationToken);
    }

    public Task<LocalWooOrderDetailsDto?> GetOrderAsync(long id, CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<LocalWooOrderDetailsDto>($"api/admin/local-orders/{id}", cancellationToken);

    public async Task ChangeStatusAsync(long id, ChangeInternalOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/local-orders/{id}/internal-status", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<StartBarookPaymentResponse?> StartBarookPaymentAsync(long orderId, StartBarookPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/barook-payments/orders/{orderId}/start", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StartBarookPaymentResponse>(cancellationToken: cancellationToken);
    }

    public async Task<VerifyBarookPaymentResponse?> VerifyBarookPaymentAsync(long sessionId, VerifyBarookPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/barook-payments/sessions/{sessionId}/verify", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VerifyBarookPaymentResponse>(cancellationToken: cancellationToken);
    }

    public async Task MarkPaymentLinkSentAsync(long sessionId, MarkBarookPaymentLinkSentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/barook-payments/sessions/{sessionId}/mark-link-sent", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ManualPaymentReceiptDto?> CreateManualReceiptAsync(long orderId, CreateManualPaymentReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/accounting/orders/{orderId}/manual-receipts", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ManualPaymentReceiptDto>(cancellationToken: cancellationToken);
    }
}
