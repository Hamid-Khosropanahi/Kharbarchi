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

    public async Task<PagedCustomerResultDto> GetCustomersPageAsync(string? search = null, string? customerType = null, bool? isBlocked = null,
        string? province = null, string? city = null, decimal? minCreditLimit = null, decimal? maxCreditLimit = null,
        decimal? minAvailableCredit = null, decimal? maxAvailableCredit = null, int page = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(customerType)) query.Add($"customerType={Uri.EscapeDataString(customerType)}");
        if (isBlocked.HasValue) query.Add($"isBlocked={isBlocked.Value.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(province)) query.Add($"province={Uri.EscapeDataString(province)}");
        if (!string.IsNullOrWhiteSpace(city)) query.Add($"city={Uri.EscapeDataString(city)}");
        if (minCreditLimit.HasValue) query.Add($"minCreditLimit={minCreditLimit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (maxCreditLimit.HasValue) query.Add($"maxCreditLimit={maxCreditLimit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (minAvailableCredit.HasValue) query.Add($"minAvailableCredit={minAvailableCredit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (maxAvailableCredit.HasValue) query.Add($"maxAvailableCredit={maxAvailableCredit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        return await _http.GetFromJsonAsync<PagedCustomerResultDto>($"api/erp/customers?{string.Join("&", query)}", cancellationToken)
            ?? new PagedCustomerResultDto([], page, pageSize, 0, 1);
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(string? search = null, CancellationToken cancellationToken = default)
        => (await GetCustomersPageAsync(search, pageSize: 100, cancellationToken: cancellationToken)).Items;

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

    public Task<(CustomerImportResultDto? Result, string? Error)> ImportCustomerDetailsAsync(
        Stream file, string fileName, string customerType, CancellationToken cancellationToken = default)
        => ImportSingleFileAsync($"api/erp/customers/import-details?customerType={Uri.EscapeDataString(customerType)}", "customersFile", file, fileName, cancellationToken);

    public Task<(CustomerImportResultDto? Result, string? Error)> ImportCustomerCreditsAsync(
        Stream file, string fileName, string customerType, CancellationToken cancellationToken = default)
        => ImportSingleFileAsync($"api/erp/customers/import-credits?customerType={Uri.EscapeDataString(customerType)}", "creditsFile", file, fileName, cancellationToken);

    private async Task<(CustomerImportResultDto? Result, string? Error)> ImportSingleFileAsync(
        string url, string fieldName, Stream file, string fileName, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(file), fieldName, fileName);
        using var response = await _http.PostAsync(url, form, cancellationToken);
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
