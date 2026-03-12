using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace CryptoSim.Blazor.Components.Pages;

public partial class Profil
{
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    private bool isEditingPassword = false;
    private bool isEditingUsername = false;

    // Champs mot de passe
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;

    // Champs username
    private string _newUsername = string.Empty;
    private string _confirmPasswordForUsername = string.Empty;

    private string? _error;
    private bool _isLoading = false;

    protected override void OnInitialized()
    {
        if (!AuthState.IsAuthenticated)
            Navigation.NavigateTo("/login");
    }

    private void OpenEditPassword()
    {
        isEditingUsername = false;
        isEditingPassword = true;
        _error = null;
    }

    private void OpenEditUsername()
    {
        isEditingPassword = false;
        isEditingUsername = true;
        _error = null;
    }

    private void CloseAllEdits()
    {
        isEditingPassword = false;
        isEditingUsername = false;
        _error = null;
    }

    private async Task HandleSave()
    {
        _error = null;
        _isLoading = true;

        Http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthState.Token);

        if (isEditingPassword)
        {
            if (_newPassword != _confirmPassword)
            {
                _error = "Les mots de passe ne correspondent pas.";
                _isLoading = false;
                return;
            }

            if (_newPassword.Length < 6)
            {
                _error = "Le mot de passe doit faire au moins 6 caractères.";
                _isLoading = false;
                return;
            }

            var response = await Http.PostAsJsonAsync("/api/auth/change-password", new
            {
                currentPassword = _currentPassword,
                newPassword = _newPassword
            });

            if (!response.IsSuccessStatusCode)
            {
                _error = "Mot de passe actuel incorrect.";
                _isLoading = false;
                return;
            }
        }
        else if (isEditingUsername)
        {
            if (string.IsNullOrWhiteSpace(_newUsername))
            {
                _error = "Le nom d'utilisateur ne peut pas être vide.";
                _isLoading = false;
                return;
            }

            var response = await Http.PostAsJsonAsync("/api/auth/change-username", new
            {
                newUsername = _newUsername,
                password = _confirmPasswordForUsername
            });

            if (!response.IsSuccessStatusCode)
            {
                _error = "Mot de passe incorrect ou nom d'utilisateur déjà pris.";
                _isLoading = false;
                return;
            }

            AuthState.Username = _newUsername;
        }

        _isLoading = false;
        CloseAllEdits();
        NotificationService.ShowSuccess("MODIFICATION ENREGISTRÉE AVEC SUCCÈS");
    }

    // --- LOGIQUE DE CALCUL DU SOLDE ---
    private decimal FondInitial => 10000m;

    private decimal Difference => AuthState.Balance - FondInitial;
    private bool IsPositive => Difference >= 0;

    private string StatusClass => IsPositive ? "green" : "red";
    private string StatusLabel => IsPositive ? "PLUS-VALUE" : "MOINS-VALUE";
    private string IconClass => IsPositive ? "bi-graph-up-arrow" : "bi-graph-down-arrow";


    // Calcul du pourcentage : ((Actuel - Initial) / Initial) * 100
    private decimal PerformancePercentage => FondInitial != 0
        ? (Difference / FondInitial) * 100
        : 0;
}