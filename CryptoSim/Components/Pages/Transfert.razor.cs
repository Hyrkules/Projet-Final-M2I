using CryptoSim.Blazor.Services;
using static System.Net.WebRequestMethods;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Transfert
    {
        private bool isDeposit = true;
        private decimal transactionAmount;
        private string? _error;
        private bool _isLoading = false;

        protected override void OnInitialized()
        {
            if (!AuthState.IsAuthenticated)
                Navigation.NavigateTo("/login");
        }

        private async Task HandleTransaction()
        {
            _error = null;

            if (transactionAmount <= 0)
            {
                _error = "Le montant doit être supérieur à 0.";
                return;
            }

            if (!isDeposit && transactionAmount > AuthState.Balance)
            {
                _error = "Solde insuffisant.";
                return;
            }

            _isLoading = true;

            try
            {
                Http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthState.Token);

                var endpoint = isDeposit ? "/api/auth/balance/credit" : "/api/auth/balance/deduct";
                var response = await Http.PostAsJsonAsync(endpoint, new { amount = transactionAmount });

                if (response.IsSuccessStatusCode)
                {
                    AuthState.Balance = isDeposit
                        ? AuthState.Balance + transactionAmount
                        : AuthState.Balance - transactionAmount;

                    string type = isDeposit ? "DÉPÔT" : "RETRAIT";
                    NotificationService.ShowSuccess($"{type} DE {transactionAmount} $ EFFECTUÉ AVEC SUCCÈS");
                    transactionAmount = 0;
                }
                else
                {
                    _error = "Erreur lors de la transaction.";
                }
            }
            catch
            {
                _error = "Impossible de contacter le serveur.";
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
