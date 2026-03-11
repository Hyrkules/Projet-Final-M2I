namespace PortfolioService.DTOs
{
    public class HoldingDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string CryptoName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal CurrentValue { get; set; }       // Quantity * CurrentPrice
        public decimal ProfitLoss { get; set; }         // CurrentValue - (Quantity * AverageBuyPrice)
        public decimal ProfitLossPercent { get; set; }  // % de gain/perte
        public DateTime UpdatedAt { get; set; }
    }
}
