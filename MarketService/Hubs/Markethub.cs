using Microsoft.AspNetCore.SignalR;

namespace Projet_CryptoSim.MarketService.Hubs;

public class MarketHub : Hub
{
    // Le serveur appelle SendPrices() pour diffuser à tous les clients connectés
    // Côté Blazor : await hubConnection.On<List<PriceUpdateDto>>("ReceivePrices", ...)
    public async Task SendPrices(object prices)
    {
        await Clients.All.SendAsync("ReceivePrices", prices);
    }
}