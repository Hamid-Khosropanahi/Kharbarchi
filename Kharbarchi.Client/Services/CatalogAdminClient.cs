using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;

namespace Kharbarchi.Client.Services;

public sealed class CatalogAdminClient
{
    private readonly HttpClient _http;

    public CatalogAdminClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CatalogLookupsDto?> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<CatalogLookupsDto>("api/admin/catalog/lookups", cancellationToken);
    }

    public async Task<IReadOnlyList<AdminProductListItemDto>> GetProductsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/admin/catalog/products"
            : $"api/admin/catalog/products?search={Uri.EscapeDataString(search.Trim())}";

        return await _http.GetFromJsonAsync<IReadOnlyList<AdminProductListItemDto>>(url, cancellationToken) ?? [];
    }

    public async Task<AdminProductDetailsDto?> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<AdminProductDetailsDto>($"api/admin/catalog/products/{id}", cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateBrandAsync(BrandUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return await _http.PostAsJsonAsync("api/admin/catalog/brands", request, cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateCommodityAsync(CommodityUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return await _http.PostAsJsonAsync("api/admin/catalog/commodities", request, cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateTagAsync(ProductTagUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return await _http.PostAsJsonAsync("api/admin/catalog/tags", request, cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateSpecDefinitionAsync(SpecDefinitionUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return await _http.PostAsJsonAsync("api/admin/catalog/spec-definitions", request, cancellationToken);
    }

    public async Task<HttpResponseMessage> CreateProductAsync(ProductUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return await _http.PostAsJsonAsync("api/admin/catalog/products", request, cancellationToken);
    }

    public async Task<HttpResponseMessage> UpdateProductAsync(int id, ProductUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return await _http.PutAsJsonAsync($"api/admin/catalog/products/{id}", request, cancellationToken);
    }
}
