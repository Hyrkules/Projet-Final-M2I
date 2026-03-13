using CryptoSim.Blazor.Services;

namespace CryptoSim.Blazor.Components.Layout
{
    public partial class MainLayout
    {
        private bool IsDark = true;

        private void ToggleTheme()
        {
            IsDark = !IsDark;
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            if (!AuthState.IsAuthenticated)
                Navigation.NavigateTo("/login");

            Notification.OnShow += (msg) => InvokeAsync(StateHasChanged);
        }
    }
}
