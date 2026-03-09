using Microsoft.EntityFrameworkCore;
using Projet_CryptoSim.MarketService.Data;
using Projet_CryptoSim.MarketService.DTOs;

namespace Projet_CryptoSim.MarketService.Services;

public class PriceService : IPriceService
{
    private readonly MarketDbContext _context;

    public PriceService(MarketDbContext context)
    {
        _context = context;
    }

    // Retourne toutes les cryptos avec leur prix actuel
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

    // Retourne une crypto par son symbole (ex: BBTC)
    public async Task<CryptoDto?> GetCryptoBySymbolAsync(string symbol)
    {
        var crypto = await _context.Cryptos
            .FirstOrDefaultAsync(c => c.Symbol.ToUpper() == symbol.ToUpper());

        if (crypto == null) return null;

        return new CryptoDto
        {
            Id = crypto.Id,
            Symbol = crypto.Symbol,
            Name = crypto.Name,
            CurrentPrice = crypto.CurrentPrice,
            LastUpdated = crypto.LastUpdated
        };
    }

    // Retourne l'historique des prix d'une crypto
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

    // Retourne un snapshot complet du marché (toutes les cryptos)
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