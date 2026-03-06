namespace MarketService.Models
{
    public class Crypto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
