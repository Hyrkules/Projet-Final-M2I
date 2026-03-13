using CryptoSim.Blazor.DTOs;
using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Statistiques
    {
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        // ==========================================================================
        // 1. ÉTATS ET DONNÉES
        // ==========================================================================
        private decimal TotalProfitLoss = 0;
        private decimal TotalProfitLossPercent = 0;
        private int CryptoCount = 0;
        private int MaxAvailableCryptos = 0;
        private string SelectedTimeFrame = "1M";
        private decimal TotalPortfolioValue => (decimal)AuthState.Balance + Holdings.Sum(h => h.CurrentValue);

        private List<HoldingDto> Holdings = new();
        private List<OrderDto> RecentOrders = new();
        private List<TransactionDto> _transactions = new();

        // ==========================================================================
        // 2. CYCLE DE VIE
        // ==========================================================================
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (!AuthState.IsAuthenticated)
                {
                    Navigation.NavigateTo("/login");
                    return;
                }

                await LoadDataAsync();
                await RenderChartsAsync();
                StateHasChanged();
            }
        }

        // ==========================================================================
        // 3. CHARGEMENT DES DONNÉES
        // ==========================================================================
        private async Task LoadDataAsync()
        {
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthState.Token);

            // Holdings
            try
            {
                var holdings = await Http.GetFromJsonAsync<List<HoldingDto>>("/api/portfolio/holdings",
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Holdings = holdings ?? new();
                CryptoCount = Holdings.Count;

                var totalValue = Holdings.Sum(h => h.CurrentValue);
                foreach (var h in Holdings)
                    h.AllocationPercent = totalValue > 0
                        ? Math.Round(h.CurrentValue / totalValue * 100, 0) : 0;
            }
            catch (Exception ex) { Console.WriteLine($">>> Erreur holdings: {ex.Message}"); }

            // Performance
            try
            {
                var perf = await Http.GetFromJsonAsync<PerformanceDto>("/api/portfolio/performance",
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TotalProfitLoss = perf?.TotalProfitLoss ?? 0;
                TotalProfitLossPercent = perf?.ProfitLossPercent ?? 0;
            }
            catch (Exception ex) { Console.WriteLine($">>> Erreur performance: {ex.Message}"); }

            // Transactions (pour le graphique de performance)
            try
            {
                _transactions = await Http.GetFromJsonAsync<List<TransactionDto>>("/api/portfolio/transactions",
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch (Exception ex) { Console.WriteLine($">>> Erreur transactions: {ex.Message}"); }

            // Ordres récents
            try
            {
                var orders = await Http.GetFromJsonAsync<List<OrderDto>>("/api/orders",
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                RecentOrders = orders?
                    .Where(o => o.Status == "Executed")
                    .OrderByDescending(o => o.ExecutedAt)
                    .Take(5).ToList() ?? new();
            }
            catch (Exception ex) { Console.WriteLine($">>> Erreur orders: {ex.Message}"); }

            try
            {
                var cryptos = await Http.GetFromJsonAsync<List<object>>("/api/market/cryptos");
                MaxAvailableCryptos = cryptos?.Count ?? 3;
            }
            catch { MaxAvailableCryptos = 3; }
        }

        // ==========================================================================
        // 4. GRAPHIQUES
        // ==========================================================================
        private async Task RenderChartsAsync()
        {
            await Task.Delay(100);

            // Camembert - Répartition des actifs
            if (Holdings.Any())
            {
                var pieData = new
                {
                    labels = Holdings.Select(h => h.CryptoSymbol).ToArray(),
                    values = Holdings.Select(h => (double)h.AllocationPercent).ToArray(),
                };
                var pieColors = new[] { "#26a69a", "#2196f3", "#673ab7", "#f59e0b" };
                await JSRuntime.InvokeVoidAsync("createPieChart", pieData, pieColors);
            }

            // Graphique de performance historique
            await LoadPerformanceChart();
        }

        private async Task LoadPerformanceChart()
        {
            // On reconstitue la courbe de valeur du portefeuille à partir des transactions
            var filtered = FilterTransactionsByTimeFrame(_transactions, SelectedTimeFrame);

            if (!filtered.Any())
            {
                await JSRuntime.InvokeVoidAsync("createLineChart", new
                {
                    dates = new[] { "Aucune donnée" },
                    values = new[] { 0 }
                });
                return;
            }

            // Cumul du total investi dans le temps
            var sorted = filtered.OrderBy(t => t.ExecutedAt).ToList();
            decimal cumul = 10_000m; // solde de départ
            var dates = new List<string>();
            var values = new List<decimal>();

            foreach (var t in sorted)
            {
                cumul += t.Type == "Buy" ? -t.Total : t.Total;
                dates.Add(t.ExecutedAt.ToString("dd MMM"));
                values.Add(Math.Round(cumul, 2));
            }

            await JSRuntime.InvokeVoidAsync("createLineChart", new
            {
                dates = dates.ToArray(),
                values = values.ToArray()
            });
        }

        private List<TransactionDto> FilterTransactionsByTimeFrame(List<TransactionDto> transactions, string timeFrame)
        {
            var cutoff = timeFrame switch
            {
                "1J" => DateTime.UtcNow.AddDays(-1),
                "1S" => DateTime.UtcNow.AddDays(-7),
                "1M" => DateTime.UtcNow.AddMonths(-1),
                _ => DateTime.MinValue // TOUS
            };
            return transactions.Where(t => t.ExecutedAt >= cutoff).ToList();
        }

        private async Task SetTimeFrame(string timeFrame)
        {
            SelectedTimeFrame = timeFrame;
            await LoadPerformanceChart();
            StateHasChanged();
        }

        private string GetCryptoImage(string symbol) => symbol switch
        {
            "BBTC" => "images/bbtc_pacifier.png",
            "PIKA" => "images/pika_head.png",
            "MOON" => "images/moon_orbit.png",
            _ => "images/bbtc_pacifier.png"
        };
    }
}