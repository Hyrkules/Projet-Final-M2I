namespace PortfolioService.DTOs
{
    public class MarketPriceDto
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
    }
}
