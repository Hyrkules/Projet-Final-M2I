using CryptoSim.Blazor.Services;
using Microsoft.JSInterop;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Home
    {
        private int? _cryptoCount;
        private int? _transactionCount;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
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
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                // Nombre de cryptos depuis MarketService
                var cryptos = await Http.GetFromJsonAsync<List<object>>("/api/market/cryptos");
                _cryptoCount = cryptos?.Count ?? 0;
            }
            catch { _cryptoCount = 0; }

            try
            {
                // Nombre de transactions depuis PortfolioService
                Http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthState.Token);

                var transactions = await Http.GetFromJsonAsync<List<object>>("/api/portfolio/transactions");
                _transactionCount = transactions?.Count ?? 0;
            }
            catch { _transactionCount = 0; }
        }
    }
}
