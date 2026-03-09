using Microsoft.AspNetCore.Mvc;
using Projet_CryptoSim.MarketService.Services;

namespace Projet_CryptoSim.MarketService.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly IPriceService _priceService;

    public MarketController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    // GET /api/market/cryptos
    // Liste de toutes les cryptos avec prix actuel — pas d'auth requise
    [HttpGet("cryptos")]
    public async Task<IActionResult> GetAllCryptos()
    {
        var cryptos = await _priceService.GetAllCryptosAsync();
        return Ok(cryptos);
    }

    // GET /api/market/cryptos/{symbol}
    // Détails d'une crypto (ex: BBTC) — pas d'auth requise
    [HttpGet("cryptos/{symbol}")]
    public async Task<IActionResult> GetCrypto(string symbol)
    {
        var crypto = await _priceService.GetCryptoBySymbolAsync(symbol);

        if (crypto == null)
            return NotFound(new { message = $"Crypto '{symbol}' introuvable." });

        return Ok(crypto);
    }

    // GET /api/market/history/{symbol}?limit=50
    // Historique des prix — pas d'auth requise
    [HttpGet("history/{symbol}")]
    public async Task<IActionResult> GetPriceHistory(string symbol, [FromQuery] int limit = 50)
    {
        var history = await _priceService.GetPriceHistoryAsync(symbol, limit);

        if (!history.Any())
            return NotFound(new { message = $"Aucun historique pour '{symbol}'." });

        return Ok(history);
    }

    // GET /api/market/snapshot
    // Snapshot complet du marché — pas d'auth requise
    [HttpGet("snapshot")]
    public async Task<IActionResult> GetSnapshot()
    {
        var snapshot = await _priceService.GetSnapshotAsync();
        return Ok(snapshot);
    }
}