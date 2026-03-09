using MarketService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Projet_CryptoSim.MarketService.Data;
using Projet_CryptoSim.MarketService.DTOs;
using Projet_CryptoSim.MarketService.Hubs;

namespace Projet_CryptoSim.MarketService.Services;

public class PriceSimulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<MarketHub> _hubContext;
    private readonly ILogger<PriceSimulatorService> _logger;

    // Interval de mise à jour : 3 secondes
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(3);

    public PriceSimulatorService(
        IServiceScopeFactory scopeFactory,
        IHubContext<MarketHub> hubContext,
        ILogger<PriceSimulatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceSimulatorService démarré.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await SimulatePricesAsync();
            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("PriceSimulatorService arrêté.");
    }

    private async Task SimulatePricesAsync()
    {
        // On crée un scope car DbContext est Scoped et le BackgroundService est Singleton
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MarketDbContext>();

        var cryptos = await context.Cryptos.ToListAsync();
        var updates = new List<PriceUpdateDto>();
        var now = DateTime.UtcNow;

        foreach (var crypto in cryptos)
        {
            // 1. Variation aléatoire entre -2% et +2%
            var variation = (decimal)(Random.Shared.NextDouble() * 4 - 2) / 100;
            crypto.CurrentPrice = Math.Max(0.01m, crypto.CurrentPrice * (1 + variation));
            crypto.LastUpdated = now;

            // 2. Insérer dans l'historique
            context.PriceHistories.Add(new PriceHistory
            {
                CryptoSymbol = crypto.Symbol,
                Price = crypto.CurrentPrice,
                RecordedAt = now
            });

            // 3. Préparer la mise à jour SignalR
            updates.Add(new PriceUpdateDto
            {
                Symbol = crypto.Symbol,
                Name = crypto.Name,
                CurrentPrice = crypto.CurrentPrice,
                LastUpdated = now
            });
        }

        // 4. Sauvegarder en base
        await context.SaveChangesAsync();

        // 5. Diffuser via SignalR à tous les clients connectés
        await _hubContext.Clients.All.SendAsync("ReceivePrices", updates);

        _logger.LogDebug("Prix mis à jour et diffusés pour {Count} cryptos.", cryptos.Count);
    }
}