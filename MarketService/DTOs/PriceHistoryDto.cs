namespace MarketService.DTOs
{
    public class PriceHistoryDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
