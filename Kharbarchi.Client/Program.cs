using Kharbarchi.Client;
using Kharbarchi.Client.Auth;
using Kharbarchi.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException("Client configuration Api:BaseUrl is missing. Add it to wwwroot/appsettings.json.");
}

builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient("KharbarchiAPI", client => {
    client.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute);
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

builder.Services.AddHttpClient("KharbarchiAPI", client => {
    client.BaseAddress = new Uri("https://localhost:7100/");
    client.Timeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddScoped<WooConnectionService>();
await builder.Build().RunAsync();





