using System.Net.Http.Json;

namespace PortfolioService.Services;

// DTO minimal pour désérialiser la réponse du MarketService
public class CryptoPrice
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
}

// ── Interface + Client MarketService ─────────────────────────────────────────

public interface IMarketServiceClient
{
    Task<CryptoPrice?> GetCryptoAsync(string symbol);
}

public class MarketServiceClient : IMarketServiceClient
{
    private readonly HttpClient _httpClient;

    public MarketServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CryptoPrice?> GetCryptoAsync(string symbol)
    {
        var response = await _httpClient.GetAsync($"/api/market/cryptos/{symbol}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<CryptoPrice>();
    }
}

// ── Interface + Client AuthService ───────────────────────────────────────────

public interface IAuthServiceClient
{
    Task<decimal> GetBalanceAsync();
}

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;

    public AuthServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetBalanceAsync()
    {
        var response = await _httpClient.GetAsync("/api/auth/balance");
        if (!response.IsSuccessStatusCode) return 0;

        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        return result?.Balance ?? 0;
    }

    private class BalanceResponse
    {
        public decimal Balance { get; set; }
    }
}