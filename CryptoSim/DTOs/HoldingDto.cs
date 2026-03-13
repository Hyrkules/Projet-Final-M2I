namespace CryptoSim.Blazor.DTOs
{
    public class HoldingDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string CryptoName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public decimal AllocationPercent { get; set; }
        public decimal AcquisitionValue => Quantity * AverageBuyPrice; // calculé, pas de setter
    }
}
