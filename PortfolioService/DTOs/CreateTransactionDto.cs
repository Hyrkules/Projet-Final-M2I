namespace PortfolioService.DTOs
{
    public class CreateTransactionDto
    {
        public int UserId { get; set; }
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PriceAtTime { get; set; }
        public decimal Total { get; set; }
    }
}
