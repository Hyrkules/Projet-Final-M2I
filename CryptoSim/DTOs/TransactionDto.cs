namespace CryptoSim.Blazor.DTOs
{
    public class TransactionDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
