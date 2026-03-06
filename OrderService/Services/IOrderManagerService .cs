using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderManagerService
{
    Task<(OrderResponseDto order, string? error)> PlaceOrderAsync(int userId, OrderRequestDto dto, string token);
    Task<List<OrderResponseDto>> GetOrdersAsync(int userId);
    Task<OrderResponseDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<(bool success, string? error)> CancelOrderAsync(int userId, int orderId);
}