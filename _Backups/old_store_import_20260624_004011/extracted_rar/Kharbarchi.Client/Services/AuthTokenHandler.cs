using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Kharbarchi.Client.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "kharbarchi_auth_token";

    public AuthTokenHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.AbsolutePath.StartsWith("/api/admin") ||
            request.RequestUri.AbsolutePath.StartsWith("/api/order"))
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }

        return response;
    }
}