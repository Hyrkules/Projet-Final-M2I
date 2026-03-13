namespace CryptoSim.Blazor.DTOs
{
    public class OrderDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExecutedAt { get; set; }
    }
}
