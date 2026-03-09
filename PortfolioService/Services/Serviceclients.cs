using System.Net.Http.Json;

namespace PortfolioService.Services;

public class CryptoPrice
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
}
