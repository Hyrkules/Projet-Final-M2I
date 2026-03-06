using System.Net.Http.Json;

namespace OrderService.Services;

// ── DTOs internes pour désérialiser les réponses des autres services ──────────

public class CryptoPrice
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
}

public class HoldingInfo
{
    public string CryptoSymbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class CreateTransactionDto
{
    public int UserId { get; set; }
    public string CryptoSymbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PriceAtTime { get; set; }
    public decimal Total { get; set; }
}

// ── Client MarketService ──────────────────────────────────────────────────────

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

// ── Client AuthService ────────────────────────────────────────────────────────

public interface IAuthServiceClient
{
    Task<decimal> GetBalanceAsync(string token);
    Task<bool> DeductBalanceAsync(int userId, decimal amount, string token);
    Task<bool> CreditBalanceAsync(int userId, decimal amount, string token);
}

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;

    public AuthServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetBalanceAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/balance");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return 0;

        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        return result?.Balance ?? 0;
    }

    public async Task<bool> DeductBalanceAsync(int userId, decimal amount, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/balance/deduct");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(new { userId, amount });

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreditBalanceAsync(int userId, decimal amount, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/balance/credit");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(new { userId, amount });

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private class BalanceResponse
    {
        public decimal Balance { get; set; }
    }
}

// ── Client PortfolioService ───────────────────────────────────────────────────

public interface IPortfolioServiceClient
{
    Task<HoldingInfo?> GetHoldingAsync(int userId, string symbol, string token);
    Task<bool> CreateTransactionAsync(CreateTransactionDto dto, string token);
}

public class PortfolioServiceClient : IPortfolioServiceClient
{
    private readonly HttpClient _httpClient;

    public PortfolioServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HoldingInfo?> GetHoldingAsync(int userId, string symbol, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/portfolio/holdings/{symbol}");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<HoldingInfo>();
    }

    public async Task<bool> CreateTransactionAsync(CreateTransactionDto dto, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/portfolio/transactions");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(dto);

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}