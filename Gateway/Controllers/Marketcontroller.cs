using Gateway.DTOs;
using Gateway.RestClient;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly string _marketBaseUrl;

    public MarketController(IConfiguration config)
    {
        _marketBaseUrl = config["Services:MarketService"] ?? "http://localhost:5002";
    }

    // GET /api/market/cryptos
    [HttpGet("cryptos")]
    public async Task<IActionResult> GetAllCryptos()
    {
        var client = new Client<CryptoDto, object>(_marketBaseUrl);
        var result = await client.GetRequestList("/api/market/cryptos");
        return Ok(result);
    }

    // GET /api/market/cryptos/{symbol}
    [HttpGet("cryptos/{symbol}")]
    public async Task<IActionResult> GetCrypto(string symbol)
    {
        var client = new Client<CryptoDto, object>(_marketBaseUrl);
        var result = await client.GetRequest($"/api/market/cryptos/{symbol}");
        return Ok(result);
    }

    // GET /api/market/history/{symbol}
    [HttpGet("history/{symbol}")]
    public async Task<IActionResult> GetHistory(string symbol, [FromQuery] int limit = 50)
    {
        var client = new Client<PriceHistoryDto, object>(_marketBaseUrl);
        var result = await client.GetRequestList($"/api/market/history/{symbol}?limit={limit}");
        return Ok(result);
    }

    // GET /api/market/snapshot
    [HttpGet("snapshot")]
    public async Task<IActionResult> GetSnapshot()
    {
        var client = new Client<CryptoDto, object>(_marketBaseUrl);
        var result = await client.GetRequestList("/api/market/snapshot");
        return Ok(result);
    }
}