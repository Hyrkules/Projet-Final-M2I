namespace PortfolioService.DTOs
{
    public class PortfolioSummaryDto
    {
        public int UserId { get; set; }
        public decimal TotalInvested { get; set; }   // somme des achats
        public decimal TotalValue { get; set; }      // valeur actuelle du portefeuille
        public decimal TotalProfitLoss { get; set; } // TotalValue - TotalInvested
        public decimal ProfitLossPercent { get; set; }
        public List<HoldingDto> Holdings { get; set; } = new();
    }
}
