using System.Net.Http.Json;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public class OrderAdminService
{
    private readonly HttpClient _http;

    public OrderAdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<OrderListDto>?> GetOrdersAsync(string? status = null)
    {
        var url = "api/admin/orderadmin";
        if (!string.IsNullOrEmpty(status))
            url += $"?status={status}";

        return await _http.GetFromJsonAsync<List<OrderListDto>>(url);
    }

    public async Task<OrderDetailDto?> GetOrderAsync(int id)
    {
        return await _http.GetFromJsonAsync<OrderDetailDto>($"api/admin/orderadmin/{id}");
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatus)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/orderadmin/{id}/status", newStatus);
        return response.IsSuccessStatusCode;
    }
}