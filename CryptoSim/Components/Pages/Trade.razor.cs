using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Trade
    {
        //  Un seul OnAfterRenderAsync
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (!AuthState.IsAuthenticated)
                {
                    Navigation.NavigateTo("/login");
                    return;
                }

                await LoadStatsAsync();

                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol);

                StateHasChanged();
            }
        }

        private string selectedSymbol = "BTCUSDT";
        private string TradeMode = "BUY";
        private double TradeAmount = 0;
        private double CurrentPrice = 1000;
        private double FakeEurBalance = 0;
        private double FakeCryptoBalance = 0;

        //  Champs nécessaires à LoadStatsAsync
        private List<HoldingDto> _holdings = new();
        private List<OrderDto> _recentOrders = new();

        private string GetSelectedCryptoImage() => selectedSymbol switch
        {
            "BTCUSDT" => "images/BBTC.png",
            "SOLUSDT" => "images/PIKA.png",
            "ETHUSDT" => "images/MOON.png",
            _ => "images/BBTC.png"
        };

        private async Task OnCryptoChanged(ChangeEventArgs e)
        {
            selectedSymbol = e.Value?.ToString() ?? "BTCUSDT";
            await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol);
        }

        private double EstimatedTotal => (TradeAmount > 0 && CurrentPrice > 0)
            ? (TradeMode == "BUY" ? (TradeAmount / CurrentPrice) : (TradeAmount * CurrentPrice))
            : 0;

        [JSInvokable]
        public void UpdateCurrentPrice(double price)
        {
            CurrentPrice = price;
            StateHasChanged();
        }

        private string GetBaseAsset() => selectedSymbol switch
        {
            "BTCUSDT" => "BBTC",
            "SOLUSDT" => "PIKA",
            "ETHUSDT" => "MOON",
            _ => "BBTC"
        };

        private void SwitchMode(string mode)
        {
            TradeMode = mode;
            TradeAmount = 0;
            StateHasChanged();
        }

        private void SetAmount(int percent)
        {
            double balance = TradeMode == "BUY" ? FakeEurBalance : FakeCryptoBalance;
            TradeAmount = Math.Round(balance * (percent / 100.0), 4);
            StateHasChanged();
        }

        private async Task ExecuteTrade()
        {
            if (TradeAmount <= 0) return;

            //  Calcul correct de la quantité selon le mode
            double quantity = TradeMode == "BUY"
                ? TradeAmount / CurrentPrice   // $ → crypto
                : TradeAmount;                 // déjà en crypto

            Console.WriteLine($">>> ExecuteTrade | Mode: {TradeMode} | Amount: {TradeAmount} | Quantity calculée: {quantity} | Price: {CurrentPrice}");

            try
            {
                Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthState.Token);

                var payload = new
                {
                    CryptoSymbol = GetBaseAsset(),
                    Type = TradeMode == "BUY" ? "Buy" : "Sell",
                    Quantity = quantity,       //  quantité réelle en crypto
                    Price = CurrentPrice
                };

                Console.WriteLine($">>> Payload : CryptoSymbol={payload.CryptoSymbol}, Type={payload.Type}, Quantity={payload.Quantity}, Price={payload.Price}");

                var response = await Http.PostAsJsonAsync("/api/orders", payload);
                Console.WriteLine($">>> Réponse : {(int)response.StatusCode} {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(">>> Ordre accepté ");
                    TradeAmount = 0;
                    await LoadStatsAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($">>> Erreur API : {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> EXCEPTION : {ex.Message}");
            }
        }

        //  Copié du même pattern que Home
        private async Task LoadStatsAsync()
        {

            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthState.Token);

            try
            {
                var raw = await Http.GetStringAsync("/api/portfolio/holdings");

                var holdings = System.Text.Json.JsonSerializer.Deserialize<List<HoldingDto>>(raw,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _holdings = holdings ?? new();

                var eurHolding = _holdings.FirstOrDefault(h => h.CryptoSymbol == "EUR");
                FakeEurBalance = (double)AuthState.Balance;

                var cryptoHolding = _holdings.FirstOrDefault(h => h.CryptoSymbol == GetBaseAsset());
                FakeCryptoBalance = (double)(cryptoHolding?.Quantity ?? 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> EXCEPTION holdings : {ex.Message}");
            }

            try
            {
                Console.WriteLine(">>> Appel /api/orders...");
                var orders = await Http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
                _recentOrders = orders?.OrderByDescending(o => o.ExecutedAt).Take(5).ToList() ?? new();
                Console.WriteLine($">>> {_recentOrders.Count} ordres récupérés");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> EXCEPTION orders : {ex.Message}");
            }

            StateHasChanged();
            Console.WriteLine(">>> LoadStatsAsync terminé ");
        }

        private class HoldingDto
        {
            public string CryptoSymbol { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public decimal CurrentValue { get; set; }
        }

        private class OrderDto
        {
            public string CryptoSymbol { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime? ExecutedAt { get; set; }
        }

        private void OnInputAmount(ChangeEventArgs e)
        {
            if (double.TryParse(e.Value?.ToString(), out var result))
            {
                TradeAmount = result;
                StateHasChanged();
            }
        }
    }
}