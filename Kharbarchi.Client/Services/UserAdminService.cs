using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;

namespace Kharbarchi.Client.Services;

public sealed class UserAdminService
{
    private readonly IHttpClientFactory _factory;
    public UserAdminService(IHttpClientFactory factory) => _factory = factory;
    private HttpClient Api => _factory.CreateClient("KharbarchiAPI");

    public async Task<IReadOnlyList<LocalRoleDto>> GetRolesAsync() => await Api.GetFromJsonAsync<List<LocalRoleDto>>("api/admin/local-users/roles") ?? [];
    public async Task<IReadOnlyList<LocalUserDto>> GetUsersAsync() => await Api.GetFromJsonAsync<List<LocalUserDto>>("api/admin/local-users") ?? [];

    public async Task<(bool IsSuccess, string Message, LocalUserDto? User)> CreateUserAsync(CreateLocalUserRequest request)
    {
        var response = await Api.PostAsJsonAsync("api/admin/local-users", request);
        return response.IsSuccessStatusCode
            ? (true, "کاربر با موفقیت ساخته شد.", await response.Content.ReadFromJsonAsync<LocalUserDto>())
            : (false, await ReadErrorAsync(response), null);
    }

    public async Task<(bool IsSuccess, string Message)> UpdateRolesAsync(string userId, IReadOnlyList<string> roles)
    {
        var response = await Api.PutAsJsonAsync($"api/admin/local-users/{Uri.EscapeDataString(userId)}/roles", new UpdateLocalUserRolesRequest { Roles = roles });
        return response.IsSuccessStatusCode ? (true, "نقش‌ها با موفقیت ذخیره شد.") : (false, await ReadErrorAsync(response));
    }

    public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(string userId, string password)
    {
        var response = await Api.PostAsJsonAsync($"api/admin/local-users/{Uri.EscapeDataString(userId)}/password", new ResetLocalUserPasswordRequest { NewPassword = password, ConfirmNewPassword = password });
        return response.IsSuccessStatusCode ? (true, "رمز عبور با موفقیت تغییر کرد.") : (false, await ReadErrorAsync(response));
    }

    public async Task<(bool IsSuccess, string Message)> SetLockStateAsync(string userId, bool isLockedOut)
    {
        var response = await Api.PostAsJsonAsync($"api/admin/local-users/{Uri.EscapeDataString(userId)}/lock-state", new SetLocalUserLockStateRequest { IsLockedOut = isLockedOut });
        return response.IsSuccessStatusCode ? (true, isLockedOut ? "حساب کاربر قفل شد." : "حساب کاربر فعال شد.") : (false, await ReadErrorAsync(response));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try { var result = await response.Content.ReadFromJsonAsync<UserAdminOperationResult>(); if (!string.IsNullOrWhiteSpace(result?.Message)) return result.Message; } catch { }
        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(text) ? $"خطای سرور: {(int)response.StatusCode}" : text;
    }
}
