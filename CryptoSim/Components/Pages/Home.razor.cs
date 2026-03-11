using CryptoSim.Blazor.Services;
using Microsoft.JSInterop;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
        "BTCUSDT" => "images/bbtc_pacifier.png",
        "SOLUSDT" => "images/pika_head.png",
        "ETHUSDT" => "images/moon_orbit.png",
        _ => "images/bbtc_pacifier.png"
    };

    private string GetCryptoImage(string symbol) => symbol switch
    {
        "BBTC" => "images/bbtc_pacifier.png",
        "PIKA" => "images/pika_head.png",
        "MOON" => "images/moon_orbit.png",
        _ => "images/bbtc_pacifier.png"
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
        try
        {
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthState.Token);

            var raw = await Http.GetStringAsync("/api/portfolio/holdings");
            Console.WriteLine($">>> Holdings JSON: {raw}"); // ← ici uniquement

            var holdings = System.Text.Json.JsonSerializer.Deserialize<List<HoldingDto>>(raw,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _holdings = holdings ?? new();
            _cryptoCount = _holdings.Count;

            var totalValue = _holdings.Sum(h => h.CurrentValue);
            if (totalValue > 0)
            {
                foreach (var h in _holdings)
                    h.AllocationPercent = Math.Round(h.CurrentValue / totalValue * 100, 0);
            }
        }
        catch (Exception ex) { Console.WriteLine($">>> Erreur holdings: {ex.Message}"); _cryptoCount = 0; }

        try
        {
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthState.Token);

            var transactions = await Http.GetFromJsonAsync<List<object>>("/api/portfolio/transactions");
            _transactionCount = transactions?.Count ?? 0;
        }
        catch { _transactionCount = 0; }

        try
        {
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthState.Token);

            var orders = await Http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
            _recentOrders = orders?.OrderByDescending(o => o.ExecutedAt).Take(5).ToList() ?? new();
        }
        catch { _recentOrders = new(); }

        try
        {

            var raw = await Http.GetStringAsync("/api/portfolio/performance");
            var perf = System.Text.Json.JsonSerializer.Deserialize<PerformanceDto>(raw,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _performance = perf?.ProfitLossPercent ?? 0;
        }
        catch (Exception ex)
        {
            _performance = 0;
        }

        try
        {
            var raw = await Http.GetStringAsync("/api/portfolio/performance");
            Console.WriteLine($">>> Performance JSON: {raw}"); // ← ici
            var perf = System.Text.Json.JsonSerializer.Deserialize<PerformanceDto>(raw,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _performance = perf?.ProfitLossPercent ?? 0;
            Console.WriteLine($">>> Performance value: {_performance}"); // ← et ici
        }
        catch (Exception ex) { Console.WriteLine($">>> Erreur performance: {ex.Message}"); _performance = 0; }
    }

    private class PerformanceDto
    {
        public decimal ProfitLossPercent { get; set; }
    }

    private class OrderDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExecutedAt { get; set; }
    }

    private class HoldingDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string CryptoName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public decimal AllocationPercent { get; set; }
    }
}