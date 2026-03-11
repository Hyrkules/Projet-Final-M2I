namespace Gateway.DTOs
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExecutedAt { get; set; }
    }
}
