using System.Net.Http.Json;
using Kharbarchi.Shared.Contracts;

namespace Kharbarchi.Client.Services;

public sealed class UserAdminService
{
    private readonly HttpClient _http;

    public UserAdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<LocalRoleDto>> GetRolesAsync()
    {
        return await _http.GetFromJsonAsync<List<LocalRoleDto>>("api/admin/local-users/roles") ?? [];
    }

    public async Task<IReadOnlyList<LocalUserDto>> GetUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<LocalUserDto>>("api/admin/local-users") ?? [];
    }

    public async Task<(bool IsSuccess, string Message, LocalUserDto? User)> CreateUserAsync(CreateLocalUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/admin/local-users", request);
        if (response.IsSuccessStatusCode)
        {
            return (true, "کاربر با موفقیت ساخته شد.", await response.Content.ReadFromJsonAsync<LocalUserDto>());
        }

        return (false, await ReadErrorAsync(response), null);
    }

    public async Task<(bool IsSuccess, string Message)> UpdateRolesAsync(string userId, IReadOnlyList<string> roles)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/local-users/{Uri.EscapeDataString(userId)}/roles", new UpdateLocalUserRolesRequest
        {
            Roles = roles
        });

        if (response.IsSuccessStatusCode)
        {
            return (true, "نقش‌ها با موفقیت ذخیره شد.");
        }

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(string userId, string password)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/local-users/{Uri.EscapeDataString(userId)}/password", new ResetLocalUserPasswordRequest
        {
            NewPassword = password,
            ConfirmNewPassword = password
        });

        if (response.IsSuccessStatusCode)
        {
            return (true, "رمز عبور با موفقیت تغییر کرد.");
        }

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool IsSuccess, string Message)> SetLockStateAsync(string userId, bool isLockedOut)
    {
        var response = await _http.PostAsJsonAsync($"api/admin/local-users/{Uri.EscapeDataString(userId)}/lock-state", new SetLocalUserLockStateRequest
        {
            IsLockedOut = isLockedOut
        });

        if (response.IsSuccessStatusCode)
        {
            return (true, isLockedOut ? "حساب کاربر قفل شد." : "حساب کاربر فعال شد.");
        }

        return (false, await ReadErrorAsync(response));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var result = await response.Content.ReadFromJsonAsync<UserAdminOperationResult>();
            if (!string.IsNullOrWhiteSpace(result?.Message))
            {
                return result.Message;
            }
        }
        catch
        {
            // Fall back to plain text below.
        }

        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(text)
            ? $"خطای سرور: {(int)response.StatusCode}"
            : text;
    }
}
