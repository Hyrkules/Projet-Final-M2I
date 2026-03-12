using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class OrderRequestDto
{
    [Required]
    public string CryptoSymbol { get; set; } = string.Empty;
    [Required]
    public string Type { get; set; } = string.Empty;  // "Buy" ou "Sell"
    [Range(0.00000001, double.MaxValue)]
    public decimal Quantity { get; set; }
}

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string CryptoSymbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
}