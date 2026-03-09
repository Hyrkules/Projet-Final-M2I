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