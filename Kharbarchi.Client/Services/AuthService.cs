using System.Net.Http.Json;
using Kharbarchi.Client.Auth;
using Kharbarchi.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Kharbarchi.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly AuthenticationStateProvider _authStateProvider;
    private const string TokenKey = "kharbarchi_auth_token";

    public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _js = js;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { UserName = username, Password = password };
        var response = await _http.PostAsJsonAsync("api/auth/login", request);

        // If API is down or returns 500
        if (!response.IsSuccessStatusCode)
        {
            return new LoginResponse { IsSuccess = false, Message = "خطا در ارتباط با سرور." };
        }

        // Read result
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result != null && result.IsSuccess && result.Token != null)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
            (_authStateProvider as JwtAuthStateProvider)?.NotifyUserAuthentication(result.Token);
            return result;
        }

        // Return the failure result with the message from the API
        return result ?? new LoginResponse { IsSuccess = false, Message = "خطای نامشخص." };
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        (_authStateProvider as JwtAuthStateProvider)?.NotifyUserLogout();
    }

    public async Task<bool> RegisterAsync(string username, string password, string? email, string? fullName)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new RegisterRequest
        {
            UserName = username,
            Password = password,
            Email = email,
            FullName = fullName
        });

        return response.IsSuccessStatusCode;
    }
}