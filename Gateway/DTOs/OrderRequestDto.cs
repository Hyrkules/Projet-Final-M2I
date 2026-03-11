using System.ComponentModel.DataAnnotations;

namespace Gateway.DTOs
{
    public class OrderRequestDto
    {
        [Required]
        public string CryptoSymbol { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty;
        [Range(0.00000001, double.MaxValue)]
        public decimal Quantity { get; set; }
    }
}
