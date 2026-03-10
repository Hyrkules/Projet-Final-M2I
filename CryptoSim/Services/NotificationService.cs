using System;
using System.Threading.Tasks;

namespace CryptoSim.Blazor.Services
{
    public class NotificationService
    {
        public event Action<string>? OnShow;
        public bool IsVisible { get; private set; }
        public string Message { get; private set; } = "";

        public void ShowSuccess(string message)
        {
            Message = message;
            IsVisible = true;
            OnShow?.Invoke(message);

            // Auto-fermeture
            _ = HideAfterDelay(10000);
        }

        public void Hide()
        {
            IsVisible = false;
            OnShow?.Invoke("");
        }

        private async Task HideAfterDelay(int delay)
        {
            await Task.Delay(delay);
            Hide();
        }
    }
}