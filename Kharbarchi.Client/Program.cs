using Kharbarchi.Client;
using Kharbarchi.Client.Auth;
using Kharbarchi.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredApiBaseUrl = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(configuredApiBaseUrl))
{
    throw new InvalidOperationException("Client configuration Api:BaseUrl is missing. Add it to wwwroot/appsettings.json.");
}

var shopUrl = builder.Configuration["Site:ShopUrl"];
if (string.IsNullOrWhiteSpace(shopUrl))
{
    throw new InvalidOperationException("Client configuration Site:ShopUrl is missing.");
}

var wooCommerceBaseUrl = builder.Configuration["WooCommerce:BaseUrl"];
if (string.IsNullOrWhiteSpace(wooCommerceBaseUrl))
{
    throw new InvalidOperationException("Client configuration WooCommerce:BaseUrl is missing.");
}

if (builder.HostEnvironment.IsDevelopment())
{
    if (!Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var developmentApiBaseUri)
        || !IsLocalDevelopmentUrl(developmentApiBaseUri.ToString()))
    {
        throw new InvalidOperationException(
            "Development Api:BaseUrl must be an absolute localhost/local-development URL.");
    }

    if (!IsLocalDevelopmentUrl(shopUrl))
    {
        throw new InvalidOperationException("Development Site:ShopUrl must be a localhost/local-development URL.");
    }

    if (!IsLocalDevelopmentUrl(wooCommerceBaseUrl))
    {
        throw new InvalidOperationException("Development WooCommerce:BaseUrl must be a localhost/local-development URL.");
    }
}
else
{
    if (!string.Equals(configuredApiBaseUrl, "/", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Production Api:BaseUrl must be exactly '/' so browser API calls stay same-origin behind Nginx.");
    }

    if (!string.Equals(shopUrl, "https://www.Kharbarchi.ir/", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Production Site:ShopUrl must be exactly 'https://www.Kharbarchi.ir/'.");
    }

    if (!string.Equals(wooCommerceBaseUrl, "https://www.Kharbarchi.ir/", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Production WooCommerce:BaseUrl must be exactly 'https://www.Kharbarchi.ir/'.");
    }
}

var apiBaseUri = builder.HostEnvironment.IsDevelopment()
    ? new Uri(configuredApiBaseUrl, UriKind.Absolute)
    : new Uri(builder.HostEnvironment.BaseAddress, UriKind.Absolute);

builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient("KharbarchiAPI", client => {
    client.BaseAddress = apiBaseUri;
    client.Timeout = TimeSpan.FromMinutes(10);
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("KharbarchiAPI"));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ProductAdminService>();
builder.Services.AddScoped<OrderAdminService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<CatalogAdminClient>();
builder.Services.AddScoped<PriceWorkflowClient>();
builder.Services.AddScoped<InventoryWorkflowClient>();
builder.Services.AddScoped<SyncOutboxClient>();
builder.Services.AddScoped<OrderWorkflowClient>();

builder.Services.AddScoped<Kharbarchi.Client.Services.WooConnectionService>();
builder.Services.AddScoped<WooConnectionService>();
await builder.Build().RunAsync();

static bool IsLocalDevelopmentUrl(string? value)
{
    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
    {
        return false;
    }

    return uri.IsLoopback
        || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
        || uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase);
}




