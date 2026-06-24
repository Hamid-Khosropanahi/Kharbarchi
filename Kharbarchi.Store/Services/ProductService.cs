using System.Net.Http.Json;
using System.Text.Json;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public class ProductService
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductService> _logger;



    public ProductService(HttpClient http, ILogger<ProductService> logger)
    {
	    _http = http;
	    _logger = logger;
    }



    public async Task<List<ProductDto>?> GetAllAsync()
    {
	    var response = await _http.GetAsync("api/product");
	    var json = await response.Content.ReadAsStringAsync();
	    _logger.LogInformation("Raw response: {Json}", json);
	    return JsonSerializer.Deserialize<List<ProductDto>>(json,
		    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }


	public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ProductDto>($"api/product/{id}");
    }
}