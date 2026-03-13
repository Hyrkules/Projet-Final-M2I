namespace CryptoSim.Blazor.Components.Pages
{
    public partial class ForgotPassword
    {
        private string email = "";
        private bool isSent = false;

        private async Task HandleReset()
        {
            // Simulation d'envoi d'email
            await Task.Delay(800);
            isSent = true;
        }
    }
}
