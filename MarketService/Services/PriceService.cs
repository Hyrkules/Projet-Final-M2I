using MarketService.DTOs;
using Microsoft.EntityFrameworkCore;
using Projet_CryptoSim.MarketService.Data;

namespace Projet_CryptoSim.MarketService.Services;

public class PriceService : IPriceService
{
    private readonly MarketdbContext _context;

    public PriceService(MarketdbContext context)
    {
        _context = context;
    }

    public async Task<List<CryptoDto>> GetAllCryptosAsync()
    {
        var cryptos = await _context.Cryptos.ToListAsync();

        return cryptos.Select(c => new CryptoDto
        {
            Id = c.Id,
            Symbol = c.Symbol,
            Name = c.Name,
            CurrentPrice = c.CurrentPrice,
            LastUpdated = c.LastUpdated
        }).ToList();
    }

    public async Task<CryptoDto?> GetCryptoBySymbolAsync(string symbol)
    {
        // 1. Récupérer l'état actuel de la crypto
        var crypto = await _context.Cryptos.FirstOrDefaultAsync(c => c.Symbol == symbol);
        if (crypto == null) return null;

        // 2. Définir la fenêtre de temps (24h)
        var dateLimite = DateTime.UtcNow.AddDays(-1);

        // 3. Calculer les stats depuis la table History
        var stats24h = await _context.PriceHistories // Remplace par le nom de ta table historique
            .Where(h => h.CryptoSymbol == symbol && h.RecordedAt >= dateLimite)
            .GroupBy(h => h.CryptoSymbol)
            .Select(g => new
            {
                High = g.Max(x => x.Price),
                Low = g.Min(x => x.Price),
                Vol = g.Sum(x => x.Price * 0.1m) // Simulation de volume (ex: 10% du prix cumulé)
            })
            .FirstOrDefaultAsync();

        // 4. Retourner le DTO complet
        return new CryptoDto
        {
            Symbol = crypto.Symbol,
            CurrentPrice = crypto.CurrentPrice,
            LastUpdated = crypto.LastUpdated,
            // Si pas d'historique (ex: nouvelle crypto), on met le prix actuel par défaut
            High24h = stats24h?.High ?? crypto.CurrentPrice,
            Low24h = stats24h?.Low ?? crypto.CurrentPrice,
            Volume24h = stats24h?.Vol ?? 0
        };
    }

    public async Task<List<PriceHistoryDto>> GetPriceHistoryAsync(string symbol, int limit = 50)
    {
        var history = await _context.PriceHistories
            .Where(p => p.CryptoSymbol.ToUpper() == symbol.ToUpper())
            .OrderByDescending(p => p.RecordedAt)
            .Take(limit)
            .Select(p => new PriceHistoryDto
            {
                CryptoSymbol = p.CryptoSymbol,
                Price = p.Price,
                RecordedAt = p.RecordedAt
            })
            .ToListAsync();

        return history;
    }

    public async Task<List<PriceUpdateDto>> GetSnapshotAsync()
    {
        var cryptos = await _context.Cryptos.ToListAsync();

        return cryptos.Select(c => new PriceUpdateDto
        {
            Symbol = c.Symbol,
            Name = c.Name,
            CurrentPrice = c.CurrentPrice,
            LastUpdated = c.LastUpdated
        }).ToList();
    }
}