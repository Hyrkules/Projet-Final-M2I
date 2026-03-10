using CryptoSim.Blazor.Services;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Login
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string? _error;
        private bool _isLoading = false;

        private async Task HandleLogin()
        {
            _isLoading = true;
            _error = null;

            var result = await AuthService.LoginAsync(_username, _password);

            if (result == null)
            {
                _error = "Identifiants incorrects.";
                _isLoading = false;
                return;
            }

            // Stocker les infos dans AuthState
            AuthState.Token = result.Token;
            AuthState.Username = result.Username;
            AuthState.Balance = result.Balance;
            AuthState.IsAuthenticated = true;

            Navigation.NavigateTo("/");
        }
    }
}
