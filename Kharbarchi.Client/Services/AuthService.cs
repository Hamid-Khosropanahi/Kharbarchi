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

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login",
            new LoginRequest { UserName = username, Password = password });

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result is not { IsSuccess: true, Token: not null })
            return false;

        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Token);
        (_authStateProvider as JwtAuthStateProvider)?.NotifyUserAuthentication(result.Token);
        return true;
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