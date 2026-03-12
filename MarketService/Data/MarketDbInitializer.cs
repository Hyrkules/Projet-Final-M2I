using MarketService.Models;

namespace Projet_CryptoSim.MarketService.Data;

public static class MarketDbInitializer
{
    public static void Initialize(MarketDbContext context)
    {
        context.Database.EnsureCreated();

        // Si des cryptos existent déjà, on ne fait rien
        if (context.Cryptos.Any()) return;

        var now = DateTime.UtcNow;

        // Seed des cryptos custom avec prix Initiaux
        var cryptos = new List<Crypto>
        {
            new() { Symbol = "BBTC",  Name = "BabyBitcoin",  CurrentPrice = 100_000m,  LastUpdated = now },
            new() { Symbol = "MOON",  Name = "MoonCoin",      CurrentPrice = 0.0042m,  LastUpdated = now },
            new() { Symbol = "PIKA",  Name = "PikaCoin",      CurrentPrice = 12.75m,   LastUpdated = now },
        };

        context.Cryptos.AddRange(cryptos);
        context.SaveChanges();

        // Historique Initial : un point de départ pour chaque crypto
        var histories = cryptos.Select(c => new PriceHistory
        {
            CryptoSymbol = c.Symbol,
            Price = c.CurrentPrice,
            RecordedAt = now
        }).ToList();

        context.PriceHistories.AddRange(histories);
        context.SaveChanges();
    }
}