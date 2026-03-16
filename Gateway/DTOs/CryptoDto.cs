namespace Gateway.DTOs
{
    public class CryptoDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public decimal Volume24h { get; set; }
    }
}
 
