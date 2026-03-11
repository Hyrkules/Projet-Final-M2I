namespace Gateway.DTOs
{
    public class PortfolioSummaryDto
    {
        public int UserId { get; set; }
        public decimal CashBalance { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal TotalGainLoss { get; set; }
        public decimal TotalGainLossPercent { get; set; }
        public List<HoldingDetailDto> Holdings { get; set; } = new();
    }
}
