using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Statistiques
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        // ==========================================================================
        // 1. ÉTATS ET DONNÉES
        // ==========================================================================
        private dynamic AuthState = new { Balance = 12500.50, IsAuthenticated = true, Token = "123" };
        private decimal TotalProfitLoss = 1250.75m;
        private decimal TotalProfitLossPercent = 10.12m;
        private int CryptoCount = 4;
        private int MaxAvailableCryptos = 10;
        private string SelectedTimeFrame = "1M";

        private List<Holding> Holdings = new List<Holding>
        {
            new Holding { CryptoSymbol = "BBTC", Quantity = 0.05m, AcquisitionValue = 2500.00m, CurrentValue = 3125.00m, ProfitLossPercent = 25.00m, AllocationPercent = 35 },
            new Holding { CryptoSymbol = "PIKA", Quantity = 1500.00m, AcquisitionValue = 3000.00m, CurrentValue = 3150.00m, ProfitLossPercent = 5.00m, AllocationPercent = 25 },
            new Holding { CryptoSymbol = "MOON", Quantity = 10.00m, AcquisitionValue = 4000.00m, CurrentValue = 3800.00m, ProfitLossPercent = -5.00m, AllocationPercent = 20 },
        };

        private List<RecentOrder> RecentOrders = new List<RecentOrder>
        {
            new RecentOrder { ExecutedAt = DateTime.Now.AddDays(-1), CryptoSymbol = "BBTC", Type = "Buy", Total = 500.00m },
            new RecentOrder { ExecutedAt = DateTime.Now.AddDays(-2), CryptoSymbol = "PIKA", Type = "Sale", Total = 1250.50m },
            new RecentOrder { ExecutedAt = DateTime.Now.AddDays(-5), CryptoSymbol = "MOON", Type = "Buy", Total = 1000.00m }
        };

        // ==========================================================================
        // 2. FONCTIONS DE RÉCUPÉRATION
        // ==========================================================================
        private string GetCryptoImage(string symbol) => $"/images/{symbol.ToLower()}.png";

        // ==========================================================================
        // 3. CYCLES DE VIE ET INTERACTIONS
        // ==========================================================================
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Un petit délai assure que le DOM est totalement prêt pour Chart.js
                await Task.Delay(100);

                // --- 1. GRAPHIQUE CAMEMBERT ---
                var pieData = new
                {
                    labels = Holdings.Select(h => h.CryptoSymbol).ToArray(),
                    values = new[] { 55, 25, 20 }, // À remplacer par h.AllocationPercent plus tard
                    offset = new[] { 40, 0, 0 }
                };
                var pieColors = new[] { "#26a69a", "#2196f3", "#673ab7" };
                await JSRuntime.InvokeVoidAsync("createPieChart", pieData, pieColors);

                // --- 2. GRAPHIQUE PERFORMANCE ---
                await LoadPerformanceChart();
            }
        }

        private async Task LoadPerformanceChart()
        {
            var perfData = new
            {
                dates = new[] { "01 Mar", "03 Mar", "05 Mar", "07 Mar", "09 Mar", "11 Mar", "12 Mar" },
                values = new[] { 10200, 10800, 10500, 11200, 11900, 12100, 12500 }
            };
            await JSRuntime.InvokeVoidAsync("createLineChart", perfData);
        }

        private async Task SetTimeFrame(string timeFrame)
        {
            SelectedTimeFrame = timeFrame;
            // Appel JS pour rafraîchir le graphique
            await LoadPerformanceChart();
            StateHasChanged(); // Force Blazor à mettre à jour les classes CSS des boutons
        }

        // ==========================================================================
        // 4. CLASSES DE DONNÉES
        // ==========================================================================
        public class Holding
        {
            public string CryptoSymbol { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public decimal AcquisitionValue { get; set; }
            public decimal CurrentValue { get; set; }
            public decimal ProfitLossPercent { get; set; }
            public int AllocationPercent { get; set; }
        }

        public class RecentOrder
        {
            public DateTime? ExecutedAt { get; set; }
            public string CryptoSymbol { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public decimal Total { get; set; }
        }
    }
}