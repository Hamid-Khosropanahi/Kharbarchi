using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts.WooCommerce;

namespace Kharbarchi.Client.Services;

public sealed class WooConnectionService
{
    private readonly IHttpClientFactory _factory;
    public WooConnectionService(IHttpClientFactory factory) => _factory = factory;
    private HttpClient Api => _factory.CreateClient("KharbarchiAPI");

    public async Task<WooConnectionSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        await Api.GetFromJsonAsync<WooConnectionSettingsDto>("api/admin/woocommerce-connection/settings", cancellationToken) ?? new();

    public async Task<WooConnectionSettingsDto> SaveSettingsAsync(WooConnectionSettingsDto settings, CancellationToken cancellationToken = default)
    {
        var response = await Api.PutAsJsonAsync("api/admin/woocommerce-connection/settings", settings, cancellationToken);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync(cancellationToken));
        return await response.Content.ReadFromJsonAsync<WooConnectionSettingsDto>(cancellationToken: cancellationToken) ?? settings;
    }

    public async Task<WooApiTestResultDto> TestAsync(WooConnectionTestRequest request, CancellationToken cancellationToken = default) =>
        await ReadResultAsync(await Api.PostAsJsonAsync("api/admin/woocommerce-connection/test", request, cancellationToken), cancellationToken);

    public async Task<WooApiTestResultDto> RawAsync(WooRawApiRequest request, CancellationToken cancellationToken = default) =>
        await ReadResultAsync(await Api.PostAsJsonAsync("api/admin/woocommerce-connection/raw", request, cancellationToken), cancellationToken);

    public async Task<WooDefaultImportResultDto> ImportDefaultDataAsync(WooDefaultImportRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await Api.PostAsJsonAsync("api/admin/woocommerce-default-import/run", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<WooDefaultImportResultDto>(cancellationToken: cancellationToken)
            ?? new WooDefaultImportResultDto { Success = false, Message = await response.Content.ReadAsStringAsync(cancellationToken) };
    }

    private static async Task<WooApiTestResultDto> ReadResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await response.Content.ReadFromJsonAsync<WooApiTestResultDto>(cancellationToken: cancellationToken)
            ?? new WooApiTestResultDto { Success = false, StatusCode = (int)response.StatusCode, Message = await response.Content.ReadAsStringAsync(cancellationToken) };
    }
}
