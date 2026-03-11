namespace Gateway.DTOs
{
    public class HoldingDetailDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string CryptoName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
