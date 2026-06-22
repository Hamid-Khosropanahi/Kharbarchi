using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Kharbarchi.Client.Auth;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider
{
    public const string TokenKey = "kharbarchi_auth_token";

    private static readonly ClaimsPrincipal AnonymousUser = new(new ClaimsIdentity());
    private readonly IJSRuntime _js;

    public JwtAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        return BuildAuthenticationState(token);
    }

    public void NotifyUserAuthentication(string token)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(token)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(AnonymousUser)));
    }

    private static AuthenticationState BuildAuthenticationState(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(AnonymousUser);
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                return new AuthenticationState(AnonymousUser);
            }

            var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(AnonymousUser);
        }
    }
}
