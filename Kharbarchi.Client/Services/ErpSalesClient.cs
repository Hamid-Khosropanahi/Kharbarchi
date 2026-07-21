using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;

namespace Kharbarchi.Client.Services;

public sealed class ErpSalesClient
{
    private readonly HttpClient _http;

    public ErpSalesClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(string? search = null, CancellationToken cancellationToken = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<CustomerListItemDto>>(
            string.IsNullOrWhiteSpace(search) ? "api/erp/customers" : $"api/erp/customers?search={Uri.EscapeDataString(search)}", cancellationToken) ?? [];

    public async Task<IReadOnlyList<ErpCatalogItemDto>> GetCatalogAsync(string? search = null, CancellationToken cancellationToken = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<ErpCatalogItemDto>>(
            string.IsNullOrWhiteSpace(search) ? "api/erp/sales/catalog" : $"api/erp/sales/catalog?search={Uri.EscapeDataString(search)}", cancellationToken) ?? [];

    public async Task<(CustomerImportResultDto? Result, string? Error)> ImportCustomersAsync(
        Stream customers,
        string customersFileName,
        Stream credits,
        string creditsFileName,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(customers), "customersFile", customersFileName);
        form.Add(new StreamContent(credits), "creditsFile", creditsFileName);
        using var response = await _http.PostAsync("api/erp/customers/import-barok", form, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (null, await response.Content.ReadAsStringAsync(cancellationToken));
        return (await response.Content.ReadFromJsonAsync<CustomerImportResultDto>(cancellationToken: cancellationToken), null);
    }

    public async Task<(ErpOrderCreatedDto? Result, string? Error)> CreateOrderAsync(ErpOrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("api/erp/sales/orders", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (null, await response.Content.ReadAsStringAsync(cancellationToken));
        return (await response.Content.ReadFromJsonAsync<ErpOrderCreatedDto>(cancellationToken: cancellationToken), null);
    }
}
