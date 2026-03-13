using System;
using System.Threading.Tasks;

namespace CryptoSim.Blazor.Services
{
    public class NotificationService
    {
        public event Action<string, bool>? OnShow;
        public bool IsVisible { get; private set; }
        public string Message { get; private set; } = "";
        public bool IsError { get; private set; } = false;

        public void ShowSuccess(string message)
        {
            IsError = false;
            Show(message);
        }

        public void ShowError(string message)
        {
            IsError = true;
            Show(message);
        }

        private void Show(string message)
        {
            Message = message;
            IsVisible = true;
            OnShow?.Invoke(message, IsError);
            _ = HideAfterDelay(4000);
        }

        public void Hide()
        {
            IsVisible = false;
            OnShow?.Invoke("", false);
        }

        private async Task HideAfterDelay(int delay)
        {
            await Task.Delay(delay);
            Hide();
        }
    }
}