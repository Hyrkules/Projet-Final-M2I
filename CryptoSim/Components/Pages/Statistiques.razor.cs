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

        // MEILLEUR TRADE (ROI) — crypto avec le meilleur % de gain sur les ventes
        private (string Symbol, decimal Percent) BestTrade
        {
            get
            {
                var result = _transactions
                    .GroupBy(t => t.CryptoSymbol)
                    .Select(g => {
                        var totalBought = g.Where(t => t.Type == "Buy").Sum(t => t.Total);
                        var totalSold = g.Where(t => t.Type == "Sell").Sum(t => t.Total);
                        var roi = totalBought > 0 ? (totalSold - totalBought) / totalBought * 100 : 0;
                        return (Symbol: g.Key, Percent: Math.Round(roi, 2));
                    })
                    .OrderByDescending(x => x.Percent)
                    .FirstOrDefault();
                return result == default ? ("--", 0) : result;
            }
        }

        // PLUS GROS GAIN — holding actuel avec le meilleur ProfitLossPercent
        private (string Symbol, decimal Percent, decimal Amount) BestCurrentHolding
        {
            get
            {
                var best = Holdings.OrderByDescending(h => h.ProfitLossPercent).FirstOrDefault();
                if (best == null) return ("--", 0, 0);
                return (best.CryptoSymbol, best.ProfitLossPercent, Math.Round(best.CurrentValue - best.AcquisitionValue, 2));
            }
        }

        // Moyenne du profit de mes trades 
        private decimal AverageProfitPerTrade
        {
            get
            {
                var sells = _transactions.Where(t => t.Type == "Sell").ToList();
                if (!sells.Any()) return 0;

                decimal totalProfit = 0;
                foreach (var sell in sells)
                {
                    // Trouve le coût moyen d'achat pour cette crypto
                    var buys = _transactions
                        .Where(t => t.Type == "Buy" && t.CryptoSymbol == sell.CryptoSymbol);

                    var avgBuyPrice = buys.Any()
                        ? buys.Sum(t => t.Total) / buys.Sum(t => t.Quantity)
                        : 0;

                    var costOfSold = avgBuyPrice * sell.Quantity;
                    totalProfit += sell.Total - costOfSold;
                }

                return Math.Round(totalProfit / sells.Count, 2);
            }
        }

        private decimal WinRate
        {
            get
            {
                int wins = 0;
                int total = 0;

                // Holdings actifs — profitable si CurrentValue > AcquisitionValue
                foreach (var h in Holdings)
                {
                    total++;
                    if (h.CurrentValue > h.AcquisitionValue) wins++;
                }

                // Positions fermées — grouper les ventes par crypto non détenue
                var soldSymbols = _transactions
                    .Where(t => t.Type == "Sell")
                    .Select(t => t.CryptoSymbol)
                    .Distinct()
                    .Where(s => !Holdings.Any(h => h.CryptoSymbol == s));

                foreach (var symbol in soldSymbols)
                {
                    var totalBought = _transactions
                        .Where(t => t.CryptoSymbol == symbol && t.Type == "Buy")
                        .Sum(t => t.Total);
                    var totalSold = _transactions
                        .Where(t => t.CryptoSymbol == symbol && t.Type == "Sell")
                        .Sum(t => t.Total);

                    total++;
                    if (totalSold > totalBought) wins++;
                }

                return total == 0 ? 0 : Math.Round((decimal)wins / total * 100, 1);
            }
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
            "BBTC" => "images/BBTC.png",
            "PIKA" => "images/PIKA.png",
            "MOON" => "images/MOON.png",
            _ => "images/BBTC.png"
        };
    }
}