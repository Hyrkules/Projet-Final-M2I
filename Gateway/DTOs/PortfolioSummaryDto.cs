namespace Gateway.DTOs
{
    public class PortfolioSummaryDto
    {
        public int UserId { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public List<HoldingDetailDto> Holdings { get; set; } = new();
    }
}
