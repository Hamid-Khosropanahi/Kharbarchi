using Kharbarchi.Client;
using Kharbarchi.Client.Auth;
using Kharbarchi.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// آدرس API – برای توسعه:
var apiBase = "https://localhost:7100/";

// HttpClient با Token Handler
builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient("KharbarchiAPI", client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("KharbarchiAPI"));

// Auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ProductAdminService>();
builder.Services.AddScoped<OrderAdminService>();
builder.Services.AddScoped<UserAdminService>();



await builder.Build().RunAsync();