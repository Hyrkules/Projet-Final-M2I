using CryptoSim.Blazor.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CryptoSim.Blazor.DTOs;

namespace CryptoSim.Blazor.Components.Pages;

public partial class Home : IDisposable // Ajout de IDisposable pour nettoyer la référence JS
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private int? _cryptoCount;
    private int? _transactionCount;
    private decimal _performancePercent = 0;
    private List<OrderDto> _recentOrders = new();
    private string selectedSymbol = "BBTC";
    private string GameMessage = "En attendant que les devs se bougent les fesses... Gagnez des PikaCoin !";
    private List<HoldingDto> _holdings = new();

    [JSInvokable]
    public async Task UpdateCurrentPrice(decimal livePrice)
    {
        _currentPrice = livePrice;

        // Si l'utilisateur est connecté, on peut mettre à jour ses holdings en direct
        if (_holdings.Any())
        {
            var currentAsset = _holdings.FirstOrDefault(h => selectedSymbol.Contains(h.CryptoSymbol));
            if (currentAsset != null)
            {
                // Logique de recalcul simplifiée pour l'affichage
                currentAsset.CurrentValue = currentAsset.Quantity * livePrice;
                StateHasChanged();
            }
        }
    }

    // Nouvelle méthode pour que le JS récupère les stats de ta base
    [JSInvokable]
    public async Task<object> GetMarketStats(string symbol)
    {
        try
        {
            var stats = await Http.GetFromJsonAsync<CryptoDto>($"/api/market/cryptos/{symbol}");
            return new
            {
                high = stats?.CurrentPrice, // À remplacer par un vrai High 24h si tu as l'info
                low = stats?.CurrentPrice,
                vol = 1250.50 // Exemple ou donnée réelle
            };
        }
        catch { return new { high = 0, low = 0, vol = 0 }; }
    }



    // Référence pour permettre au JavaScript d'appeler des méthodes C#
    private DotNetObjectReference<Home>? _objRef;

    private decimal _currentPrice = 0;

    private async Task LoadCurrentPriceAsync(string symbol)
    {
        try
        {
            var crypto = await Http.GetFromJsonAsync<CryptoDto>($"/api/market/cryptos/{symbol}",
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _currentPrice = crypto?.CurrentPrice ?? 0;
        }
        catch { _currentPrice = 0; }
    }

    private class CryptoDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
    }


    private string GetSelectedCryptoImage() => selectedSymbol switch
    {
        "BBTC" => "images/BBTC.png",
        "PIKA" => "images/PIKA.png",
        "MOON" => "images/MOON.png",
        _ => "images/BBTC.png"
    };

    private string GetCryptoImage(string symbol) => symbol switch
    {
        "BBTC" => "images/BBTC.png",
        "PIKA" => "images/PIKA.png",
        "MOON" => "images/MOON.png",
        _ => "images/BBTC.png"
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);

            await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol, _objRef);
     

            if (AuthState.IsAuthenticated)
            {
                await LoadStatsAsync();
            }

            try { await JSRuntime.InvokeVoidAsync("initDinoGame"); } catch { }

            StateHasChanged();
        }
    }

    private async Task OnCryptoChanged(ChangeEventArgs e)
    {
        var newSymbol = e.Value?.ToString() ?? "BBTC";
        if (newSymbol == selectedSymbol) return;

        selectedSymbol = newSymbol;

        // On force la mise à jour de l'image de l'icône immédiatement
        StateHasChanged();

        // Petit délai pour laisser le DOM se stabiliser
        await Task.Delay(50);

        // On relance la création (le JS va tout nettoyer grâce au remove() ci-dessus)
        await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol, _objRef);
    }


    [JSInvokable]
    public void UpdatePikaCoins(int score)
    {
        int totalGains = score * 1;
        GameMessage = $"Vous avez gagné {totalGains} PikaCoin !";
        StateHasChanged();
    }

    private async Task LoadStatsAsync()
    {
        Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AuthState.Token);

        try
        {
            _holdings = await Http.GetFromJsonAsync<List<HoldingDto>>("/api/portfolio/holdings",
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            _cryptoCount = _holdings.Count;

            var totalValue = _holdings.Sum(h => h.CurrentValue);
            if (totalValue > 0)
            {
                foreach (var h in _holdings)
                    h.AllocationPercent = Math.Round(h.CurrentValue / totalValue * 100, 0);
            }
        }
        catch (Exception ex) { Console.WriteLine($">>> Erreur holdings: {ex.Message}"); }

        try
        {
            var transactions = await Http.GetFromJsonAsync<List<object>>("/api/portfolio/transactions");
            _transactionCount = transactions?.Count ?? 0;
        }
        catch { _transactionCount = 0; }

        try
        {
            var orders = await Http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
            _recentOrders = orders?.OrderByDescending(o => o.ExecutedAt).Take(5).ToList() ?? new();
        }
        catch { _recentOrders = new(); }

        try
        {
            var perf = await Http.GetFromJsonAsync<PerformanceDto>("/api/portfolio/performance",
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _performancePercent = perf?.ProfitLossPercent ?? 0;
        }
        catch (Exception) { _performancePercent = 0; }

        StateHasChanged();
    }

    private async Task FocusGame()
    {
        await JSRuntime.InvokeVoidAsync("eval", "document.getElementById('dino-game-wrapper').focus()");
    }

    private static DateTime ToParisTime(DateTime utcDate)
    {
        var parisZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, parisZone);
    }

    public void Dispose()
    {
        // Libère la mémoire lors de la fermeture de la page
        _objRef?.Dispose();
    }
}