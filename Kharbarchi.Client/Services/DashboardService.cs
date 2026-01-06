using System.Net.Http.Json;

namespace Kharbarchi.Client.Services;

public class DashboardService
{
    private readonly HttpClient _http;

    public DashboardService(HttpClient http)
    {
        _http = http;
    }

    public async Task<DashboardStats?> GetStatsAsync()
    {
        return await _http.GetFromJsonAsync<DashboardStats>("api/admin/dashboard/stats");
    }

    public async Task<List<LatestOrderDto>?> GetLatestOrdersAsync()
    {
        return await _http.GetFromJsonAsync<List<LatestOrderDto>>("api/admin/dashboard/latest-orders");
    }
}

public class DashboardStats
{
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodaySales { get; set; }
    public decimal MonthSales { get; set; }
    public int Users { get; set; }
    public int Products { get; set; }
}

public class LatestOrderDto
{
    public int Id { get; set; }
    public string Customer { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}