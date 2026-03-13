using MarketService.DTOs;

namespace Projet_CryptoSim.MarketService.Services;

public interface IPriceService
{
    Task<List<CryptoDto>> GetAllCryptosAsync();
    Task<CryptoDto?> GetCryptoBySymbolAsync(string symbol);
    Task<List<PriceHistoryDto>> GetPriceHistoryAsync(string symbol, int limit = 50);
    Task<List<PriceUpdateDto>> GetSnapshotAsync();
}