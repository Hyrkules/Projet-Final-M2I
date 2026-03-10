namespace MarketService.Models
{
    public class PriceHistory
    {
        public int Id { get; set; }
        public string CryptoSymbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
