using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class NotFound
    {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private string GameMessage = "En attendant que les devs se bougent les fesses... Gagnez des PikaCoin !";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initDinoGame");
            }
        }

        private async Task FocusGame()
        {
            await JSRuntime.InvokeVoidAsync(
                "eval",
                "document.getElementById('dino-game-wrapper').focus()"
            );
        }

        [JSInvokable]
        public void UpdatePikaCoins(int score)
        {
            GameMessage = $"Vous avez gagné {score} PikaCoin !";
            StateHasChanged();
        }
    }
}