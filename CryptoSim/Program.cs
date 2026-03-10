using CryptoSim.Blazor.Components;
using CryptoSim.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var gatewayUrl = builder.Configuration["GatewayBaseUrl"] ?? "http://localhost:5005";

builder.Services.AddScoped<CryptoSim.Blazor.Services.NotificationService>();

builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Gateway"));

builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
