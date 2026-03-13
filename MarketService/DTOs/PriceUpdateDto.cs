namespace MarketService.DTOs
{
    public class PriceUpdateDto
    {
        // Utilisé par SignalR pour diffuser les mises à jour en temps réel
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
