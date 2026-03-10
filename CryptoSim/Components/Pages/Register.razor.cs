using CryptoSim.Blazor.Services;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Register
    {
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string? _error;
        private bool _isLoading = false;

        private async Task HandleRegister()
        {
            _error = null;

            // Validation côté client
            if (_password != _confirmPassword)
            {
                _error = "Les mots de passe ne correspondent pas.";
                return;
            }

            if (_password.Length < 6)
            {
                _error = "Le mot de passe doit faire au moins 6 caractères.";
                return;
            }

            _isLoading = true;

            var result = await AuthService.RegisterAsync(_username, _email, _password);

            if (result == null)
            {
                _error = "Erreur lors de la création du compte. Nom d'utilisateur ou email déjà utilisé.";
                _isLoading = false;
                return;
            }

            // Connexion automatique après inscription
            AuthState.Token = result.Token;
            AuthState.Username = result.Username;
            AuthState.Balance = result.Balance;
            AuthState.IsAuthenticated = true;

            Navigation.NavigateTo("/");
        }
    }
}
