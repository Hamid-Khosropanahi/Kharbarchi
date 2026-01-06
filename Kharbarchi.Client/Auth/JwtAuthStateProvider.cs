using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Kharbarchi.Client.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "kharbarchi_auth_token";

    public JwtAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        var identity = new ClaimsIdentity(claims, "jwt");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
}