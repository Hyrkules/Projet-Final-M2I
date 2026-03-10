namespace OrderService.DTOs
{
    public class ExternalDTO
    {
        public class CryptoPrice
        {
            public string Symbol { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public decimal CurrentPrice { get; set; }
        }

        public class HoldingInfo
        {
            public string CryptoSymbol { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
        }

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
}
