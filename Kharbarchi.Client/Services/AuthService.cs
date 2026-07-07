using System.Net.Http.Json;
using Kharbarchi.Client.Auth;
using Kharbarchi.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Kharbarchi.Client.Services;

public sealed class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _js = js;
        _authStateProvider = (JwtAuthStateProvider)authStateProvider;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                UserName = username.Trim(),
                Password = password
            };

            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (!response.IsSuccessStatusCode)
            {
                return result ?? new LoginResponse { IsSuccess = false, Message = "نام کاربری یا رمز عبور اشتباه است." };
            }

            if (result?.IsSuccess == true && !string.IsNullOrWhiteSpace(result.Token))
            {
                await StoreTokenAsync(result.Token);
                return result;
            }

            return result ?? new LoginResponse { IsSuccess = false, Message = "خطای نامشخص در ورود." };
        }
        catch (HttpRequestException)
        {
            return new LoginResponse { IsSuccess = false, Message = "خطا در ارتباط با سرور." };
        }
        catch (TaskCanceledException)
        {
            return new LoginResponse { IsSuccess = false, Message = "زمان پاسخ‌گویی سرور تمام شد." };
        }
    }

    public async Task LogoutAsync()
    {
        foreach (var key in JwtAuthStateProvider.TokenStorageKeys)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", key);
        }

        _http.DefaultRequestHeaders.Authorization = null;
        _authStateProvider.NotifyUserLogout();
    }

    // این امضا برای سازگاری با صفحات فعلی پروژه حفظ شده است.
    public async Task<bool> RegisterAsync(string username, string password, string? email, string? fullName)
    {
        var result = await RegisterWithResponseAsync(username, password, email, fullName);
        return result.IsSuccess;
    }

    public async Task<LoginResponse> RegisterWithResponseAsync(string username, string password, string? email, string? fullName)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new RegisterRequest
            {
                UserName = username.Trim(),
                Password = password,
                Email = email?.Trim(),
                FullName = fullName?.Trim()
            });

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (!response.IsSuccessStatusCode)
            {
                return result ?? new LoginResponse { IsSuccess = false, Message = "ثبت‌نام ناموفق بود." };
            }

            if (result?.IsSuccess == true && !string.IsNullOrWhiteSpace(result.Token))
            {
                await StoreTokenAsync(result.Token);
            }

            return result ?? new LoginResponse { IsSuccess = false, Message = "خطای نامشخص در ثبت‌نام." };
        }
        catch (HttpRequestException)
        {
            return new LoginResponse { IsSuccess = false, Message = "خطا در ارتباط با سرور." };
        }
        catch (TaskCanceledException)
        {
            return new LoginResponse { IsSuccess = false, Message = "زمان پاسخ‌گویی سرور تمام شد." };
        }
    }

    private async Task StoreTokenAsync(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", JwtAuthStateProvider.TokenKey, token);
        _authStateProvider.NotifyUserAuthentication(token);
    }
}
