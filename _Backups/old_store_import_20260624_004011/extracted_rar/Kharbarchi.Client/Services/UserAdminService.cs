using System.Net.Http.Json;

namespace Kharbarchi.Client.Services;

public class UserAdminService
{
    private readonly HttpClient _http;

    public UserAdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UserListDto>?> GetUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<UserListDto>>("api/admin/useradmin");
    }

    public async Task<UserDetailDto?> GetUserAsync(string id)
    {
        return await _http.GetFromJsonAsync<UserDetailDto>($"api/admin/useradmin/{id}");
    }

    public async Task<bool> UpdateRolesAsync(string id, List<string> roles)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/useradmin/{id}/roles", roles);
        return response.IsSuccessStatusCode;
    }
}

public class UserListDto
{
    public string Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string? FullName { get; set; }
}

public class UserDetailDto
{
    public string Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public List<string> Roles { get; set; } = new();
}