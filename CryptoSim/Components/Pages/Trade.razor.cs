using Microsoft.JSInterop;

namespace CryptoSim.Blazor.Components.Pages
{
    public partial class Trade 
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // On attend 100ms que le CSS Glassmorphism soit bien appliqué
                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("createCryptoChart", "cryptoChart");

            }
        }
    }
}