using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public sealed class SyncOutboxClient
{
    private readonly HttpClient _http;

    public SyncOutboxClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<SyncOutboxDto>> GetOutboxAsync(OutboxStatus? status = null, CancellationToken cancellationToken = default)
    {
        var url = status.HasValue ? $"api/central-sync/outbox?status={status.Value}" : "api/central-sync/outbox";
        return await _http.GetFromJsonAsync<IReadOnlyList<SyncOutboxDto>>(url, cancellationToken) ?? [];
    }
}
