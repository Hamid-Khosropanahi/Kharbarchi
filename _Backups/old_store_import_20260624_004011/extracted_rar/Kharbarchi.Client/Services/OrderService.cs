using System.Net.Http.Json;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public class OrderService
{
    private readonly HttpClient _http;

    public OrderService(HttpClient http)
    {
        _http = http;
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/order", request);
        if (!response.IsSuccessStatusCode)
            return 0;

        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<OrderDetailDto?> GetOrderAsync(int id)
    {
        return await _http.GetFromJsonAsync<OrderDetailDto>($"api/order/{id}");
    }
}