namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CryptoSymbol { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }        // prix au moment de l'ordre
    public decimal Total { get; set; }        // Quantity * Price
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
}

public enum OrderType { Buy, Sell }
public enum OrderStatus { Pending, Executed, Cancelled, Rejected }