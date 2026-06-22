using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public sealed class PriceWorkflowClient
{
    private readonly HttpClient _http;

    public PriceWorkflowClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<PriceProposalDto>> GetProposalsAsync(WorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var url = status.HasValue ? $"api/admin/pricing/proposals?status={status.Value}" : "api/admin/pricing/proposals";
        return await _http.GetFromJsonAsync<IReadOnlyList<PriceProposalDto>>(url, cancellationToken) ?? [];
    }

    public Task<HttpResponseMessage> CreateProposalAsync(PriceProposalCreateRequest request, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync("api/admin/pricing/proposals", request, cancellationToken);

    public Task<HttpResponseMessage> ApproveManagerAsync(long id, string? note, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync($"api/admin/pricing/proposals/{id}/approve-manager", new ApprovalRequest { Note = note }, cancellationToken);

    public Task<HttpResponseMessage> ApproveSuperAdminAsync(long id, string? note, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync($"api/admin/pricing/proposals/{id}/approve-superadmin", new ApprovalRequest { Note = note }, cancellationToken);

    public Task<HttpResponseMessage> RejectAsync(long id, string reason, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync($"api/admin/pricing/proposals/{id}/reject", new RejectRequest { Reason = reason }, cancellationToken);
}
