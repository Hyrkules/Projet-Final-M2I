namespace Gateway.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PriceAtTime { get; set; }
        public decimal Total { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
