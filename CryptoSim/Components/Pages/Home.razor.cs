using CryptoSim.Blazor.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CryptoSim.Blazor.DTOs;

namespace CryptoSim.Blazor.Components.Pages;

public partial class Home
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private int? _cryptoCount;
    private int? _transactionCount;
    private decimal _performance = 0;
    private List<OrderDto> _recentOrders = new();
    private string selectedSymbol = "BTCUSDT";
    private string GameMessage = "En attendant que les devs se bougent les fesses... Gagnez des PikaCoin !";
    private List<HoldingDto> _holdings = new();

    private string GetSelectedCryptoImage() => selectedSymbol switch
    {
        "BTCUSDT" => "images/BBTC.png",
        "SOLUSDT" => "images/PIKA.png",
        "ETHUSDT" => "images/MOON.png",
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
            if (!AuthState.IsAuthenticated)
            {
                // On attend 100ms que le CSS Glassmorphism soit bien appliqué
                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart");

                try
                {
                    await JSRuntime.InvokeVoidAsync("initDinoGame");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur JS : {ex.Message}");
                }
            }

            await LoadStatsAsync();

            await Task.Delay(500);

            await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol);

            try
            {
                await JSRuntime.InvokeVoidAsync("initDinoGame");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur JS : {ex.Message}");
            }

            StateHasChanged();
        }
    }

    private async Task OnCryptoChanged(ChangeEventArgs e)
    {
        selectedSymbol = e.Value?.ToString() ?? "BTCUSDT";
        await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol);
    }

    private async Task FocusGame()
    {
        await JSRuntime.InvokeVoidAsync("eval", "document.getElementById('dino-game-wrapper').focus()");
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
            _performance = perf?.ProfitLossPercent ?? 0;
        }
        catch (Exception ex) { _performance = 0; }

        StateHasChanged();
    }

    private static DateTime ToParisTime(DateTime utcDate)
    {
        var parisZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, parisZone);
    }
}