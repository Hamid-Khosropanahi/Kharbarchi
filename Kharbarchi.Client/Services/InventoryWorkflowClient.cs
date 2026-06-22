using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public sealed class InventoryWorkflowClient
{
    private readonly HttpClient _http;

    public InventoryWorkflowClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<InventoryProposalDto>> GetProposalsAsync(WorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var url = status.HasValue ? $"api/admin/inventory/proposals?status={status.Value}" : "api/admin/inventory/proposals";
        return await _http.GetFromJsonAsync<IReadOnlyList<InventoryProposalDto>>(url, cancellationToken) ?? [];
    }

    public Task<HttpResponseMessage> CreateProposalAsync(InventoryProposalCreateRequest request, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync("api/admin/inventory/proposals", request, cancellationToken);

    public Task<HttpResponseMessage> ApproveManagerAsync(long id, string? note, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync($"api/admin/inventory/proposals/{id}/approve-manager", new ApprovalRequest { Note = note }, cancellationToken);

    public Task<HttpResponseMessage> ApproveSuperAdminAsync(long id, string? note, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync($"api/admin/inventory/proposals/{id}/approve-superadmin", new ApprovalRequest { Note = note }, cancellationToken);

    public Task<HttpResponseMessage> RejectAsync(long id, string reason, CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync($"api/admin/inventory/proposals/{id}/reject", new RejectRequest { Reason = reason }, cancellationToken);
}
