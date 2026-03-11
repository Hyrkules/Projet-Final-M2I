using CryptoSim.Blazor.Components;
using CryptoSim.Blazor.Services;
// Ajoute Blazored.LocalStorage si tu l'utilises (très recommandé)
// using Blazored.LocalStorage; 

var builder = WebApplication.CreateBuilder(args);

// 1. Services de base
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Configuration de l'URL Gateway
// Note : Dans Docker, "gateway" est le nom du service défini dans ton docker-compose
var gatewayUrl = builder.Configuration["GatewayBaseUrl"] ?? "http://localhost:5005";

builder.Services.AddScoped<NotificationService>();

// 3. OPTIMISATION : Configuration HttpClient
// Cette ligne permet d'injecter directement HttpClient dans tes services (AuthService, etc.)
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
});

// Si tu as d'autres services, fais de même :
// builder.Services.AddHttpClient<MarketService>(client => client.BaseAddress = new Uri(gatewayUrl));

// 4. (Optionnel mais conseillé) Gestion du stockage du Token
// builder.Services.AddBlazoredLocalStorage(); 

var app = builder.Build();

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