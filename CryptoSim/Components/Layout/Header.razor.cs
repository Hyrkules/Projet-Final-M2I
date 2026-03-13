using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace CryptoSim.Blazor.Components.Layout
{
    public partial class Header
    {
        [Parameter] public EventCallback OnThemeToggled { get; set; }

        private bool _localDark = true;
        private bool isMenuOpen = false;

        // Pour tester : 
        //private dynamic AuthState = new { Username = "Utilisateur Démo", Balance = 1250.50, IsAuthenticated = true, Token = "123" };

        private string _Initial => string.IsNullOrEmpty(AuthState.Username)
            ? "?"
            : AuthState.Username[0].ToString().ToUpper();

        private async Task HandleToggle()
        {
            _localDark = !_localDark;
            await OnThemeToggled.InvokeAsync();
        }

        private void ToggleMenu() => isMenuOpen = !isMenuOpen;

        // Ajout d'un délai pour CloseMenu pour permettre aux clics sur les liens du menu de fonctionner avant la fermeture
        private async Task CloseMenu()
        {
            await Task.Delay(150);
            isMenuOpen = false;
            StateHasChanged();
        }

        private void Logout()
        {
            // Votre logique de déconnexion ici
            isMenuOpen = false;
            Navigation.NavigateTo("/login");
        }


        // Logique pour mettre à jour le solde en temps réel
        protected override void OnInitialized()
        {
            AuthState.OnBalanceChanged += OnBalanceUpdated;
        }

        private void OnBalanceUpdated()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            AuthState.OnBalanceChanged -= OnBalanceUpdated;
        }
    }
}
