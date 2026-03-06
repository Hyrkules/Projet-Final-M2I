namespace PortfolioService.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Buy" ou "Sell"
        public decimal Quantity { get; set; }
        public decimal PriceAtTime { get; set; }
        public decimal Total { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}
