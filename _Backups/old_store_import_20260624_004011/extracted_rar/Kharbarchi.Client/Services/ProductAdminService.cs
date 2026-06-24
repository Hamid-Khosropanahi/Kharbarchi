using System.Net.Http.Json;
//using Kharbarchi.Server;
//using Kharbarchi.Server.Models; // اگر نمی‌خواهی مدل سرور را Share کنی، می‌توانی یک AdminProductDto جدا بسازی
using Kharbarchi.Shared.Models;
using System.Net.Http.Json;
namespace Kharbarchi.Client.Services;

public class ProductAdminService
{
    private readonly HttpClient _http;

    public ProductAdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Product>?> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<Product>>("api/admin/productadmin");
    }

    public async Task<Product?> GetAsync(int id)
    {
        return await _http.GetFromJsonAsync<Product>($"api/admin/productadmin/{id}");
    }

    public async Task<bool> CreateAsync(Product product)
    {
        var response = await _http.PostAsJsonAsync("api/admin/productadmin", product);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/productadmin/{product.Id}", product);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/admin/productadmin/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> UploadImageAsync(MultipartFormDataContent content)
    {
        var response = await _http.PostAsync("api/admin/productadmin/upload-image", content);
        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
        return result?.Url;
    }
}

public class ImageUploadResult
{
    public string Url { get; set; } = default!;
}