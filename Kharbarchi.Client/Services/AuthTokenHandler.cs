using System.Net.Http.Headers;
using Kharbarchi.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Kharbarchi.Client.Services;

public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthTokenHandler(IJSRuntime js, AuthenticationStateProvider authenticationStateProvider)
    {
        _js = js;
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", JwtAuthStateProvider.TokenKey);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", JwtAuthStateProvider.TokenKey);
            if (_authenticationStateProvider is JwtAuthStateProvider jwtAuthStateProvider)
            {
                jwtAuthStateProvider.NotifyUserLogout();
            }
        }

        return response;
    }
}
