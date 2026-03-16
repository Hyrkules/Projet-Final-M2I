using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Trade : IDisposable
    {
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        private string selectedSymbol = "BBTC";
        private string TradeMode = "BUY";
        private decimal TradeAmount = 0; // Quantité de Crypto
        private decimal FakeEurBalance = 0;
        private decimal FakeCryptoBalance = 0;
        private decimal _currentPrice = 0;

        private DotNetObjectReference<Trade>? _objRef;

        // Somme totale en $ (Quantité * Prix)
        private decimal EstimatedTotal => TradeAmount * _currentPrice;

        private List<HoldingDto> _holdings = new();
        private List<OrderDto> _recentOrders = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (!AuthState.IsAuthenticated)
                {
                    Navigation.NavigateTo("/login");
                    return;
                }

                _objRef = DotNetObjectReference.Create(this);
                await LoadStatsAsync();

                // On initialise le graphique et on passe la référence pour le prix live
                await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol, _objRef);

                StateHasChanged();
            }
        }

        [JSInvokable]
        public void UpdateCurrentPrice(decimal livePrice)
        {
            _currentPrice = livePrice;
            StateHasChanged();
        }

        private async Task OnCryptoChanged(ChangeEventArgs e)
        {
            selectedSymbol = e.Value?.ToString() ?? "BBTC";
            TradeAmount = 0; // Reset montant

            // Recharger les stats pour la nouvelle crypto (solde spécifique)
            await LoadStatsAsync();

            // Mettre à jour le graphique
            await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart", selectedSymbol, _objRef);
        }

        private void SwitchMode(string mode)
        {
            TradeMode = mode;
            TradeAmount = 0;
            StateHasChanged();
        }

        private void SetAmount(int percent)
        {
            if (_currentPrice <= 0)
            {
                return;
            }

            decimal calculatedAmount = 0;
            if (TradeMode == "BUY")
            {
                decimal budget = (FakeEurBalance * (percent / 100m)) * 0.999m;
                calculatedAmount = budget / _currentPrice;
            }
            else
            {
                // Calcul : Solde Crypto * pourcentage
                calculatedAmount = FakeCryptoBalance * (percent / 100m);
            }
            TradeAmount = Math.Round(calculatedAmount, 3);
            StateHasChanged();
        }

        private async Task ExecuteTrade()
        {
            // 1. Validation de base
            if (TradeAmount <= 0)
            {
                NotificationService.ShowError("Veuillez saisir une quantité supérieure à 0.");
                return;
            }

            if (_currentPrice <= 0)
            {
                NotificationService.ShowError("Le prix du marché n'est pas encore disponible. Veuillez patienter.");
                return;
            }

            // 2. Calcul du coût et vérification des soldes (Validation pré-envoi)
            decimal totalCost = TradeAmount * _currentPrice;

            if (TradeMode == "BUY")
            {
                // On vérifie si l'utilisateur a assez de cash (incluant une petite marge pour d'éventuels frais côté serveur)
                if (totalCost > FakeEurBalance)
                {
                    NotificationService.ShowError($"Solde insuffisant - Coût estimé : {totalCost:N2} $, mais vous ne possédez que {FakeEurBalance:N2} $.");
                    return;
                }
            }
            else // SELL
            {
                if (TradeAmount > FakeCryptoBalance)
                {
                    NotificationService.ShowError($"Stock de {selectedSymbol} insuffisant. Vous essayez de vendre {TradeAmount} mais vous n'avez que {FakeCryptoBalance} en portefeuille.");
                    return;
                }
            }

            // 3. Préparation des données pour l'envoi
            decimal executedAmount = TradeAmount;
            decimal executedPrice = _currentPrice;
            string actionText = TradeMode == "BUY" ? "D'ACHAT" : "DE VENTE";

            var orderRequest = new
            {
                CryptoSymbol = selectedSymbol,
                Type = TradeMode == "BUY" ? "Buy" : "Sell",
                Quantity = executedAmount,
                Price = executedPrice
            };

            try
            {
                var response = await Http.PostAsJsonAsync("/api/orders", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    TradeAmount = 0;
                    await LoadStatsAsync(); // Met à jour les soldes après l'ordre

                    // Message de succès complet
                    NotificationService.ShowSuccess(
                        $"ORDRE {actionText} RÉUSSI - {executedAmount} {selectedSymbol} à {executedPrice:N2} $ (Total: {totalCost:N2} $)");
                }
                else
                {
                    // Lecture de l'erreur envoyée par le serveur (ex: erreur métier du backend)
                    var errorBody = await response.Content.ReadAsStringAsync();

                    // Si le serveur renvoie un objet d'erreur standard ASP.NET, on essaie de l'extraire, 
                    // sinon on affiche le corps de la réponse.
                    NotificationService.ShowError($"Le serveur a refusé l'ordre : {errorBody}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowError("Impossible de contacter le service de trading. Vérifiez votre connexion.");
                Console.WriteLine($">>> EXCEPTION : {ex.Message}");
            }
        }

        private string GetBaseAsset() => selectedSymbol;

        private string GetSelectedCryptoImage() => selectedSymbol switch
        {
            "BBTC" => "images/BBTC.png",
            "PIKA" => "images/PIKA.png",
            "MOON" => "images/MOON.png",
            _ => "images/BBTC.png"
        };

        private async Task LoadStatsAsync()
        {
            if (string.IsNullOrEmpty(AuthState.Token)) return;

            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthState.Token);

            try
            {
                _holdings = await Http.GetFromJsonAsync<List<HoldingDto>>("/api/portfolio/holdings") ?? new();

                // Mise à jour des soldes pour l'UI
                FakeEurBalance = AuthState.Balance; // Le cash vient du profil global

                var cryptoHolding = _holdings.FirstOrDefault(h => h.CryptoSymbol == selectedSymbol);
                FakeCryptoBalance = cryptoHolding?.Quantity ?? 0;
            }
            catch (Exception ex) { Console.WriteLine($"Err holdings: {ex.Message}"); }

            try
            {
                var orders = await Http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
                _recentOrders = orders?.OrderByDescending(o => o.ExecutedAt).Take(5).ToList() ?? new();
            }
            catch (Exception ex) { Console.WriteLine($"Err orders: {ex.Message}"); }

            StateHasChanged();
        }

        // Classes de données (DTO)
        public class HoldingDto
        {
            public string CryptoSymbol { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public decimal CurrentValue { get; set; }
        }

        public class OrderDto
        {
            public string CryptoSymbol { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime? ExecutedAt { get; set; }
        }

        public void Dispose() => _objRef?.Dispose();
    }
}