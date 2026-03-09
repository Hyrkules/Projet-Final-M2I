namespace Projet_CryptoSim.MarketService.DTOs;

public class CryptoDto
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PriceHistoryDto
{
    public string CryptoSymbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime RecordedAt { get; set; }
}

// Utilisé par SignalR pour diffuser les mises à jour en temps réel
public class PriceUpdateDto
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}