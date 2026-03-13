using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using static OrderService.DTOs.ExternalDto;

namespace OrderService.Services;

public class OrderManager : IOrderManagerService
{
    private readonly OrderdbContext _context;
    private readonly IMarketServiceClient _marketClient;
    private readonly IAuthServiceClient _authClient;
    private readonly IPortfolioServiceClient _portfolioClient;

    public OrderManager(
        OrderdbContext context,
        IMarketServiceClient marketClient,
        IAuthServiceClient authClient,
        IPortfolioServiceClient portfolioClient)
    {
        _context = context;
        _marketClient = marketClient;
        _authClient = authClient;
        _portfolioClient = portfolioClient;
    }

    public async Task<(OrderResponseDto order, string? error)> PlaceOrderAsync(int userId, OrderRequestDto dto, string token)
    {
        var crypto = await _marketClient.GetCryptoAsync(dto.CryptoSymbol);
        if (crypto == null)
            return (null!, $"Crypto '{dto.CryptoSymbol}' introuvable.");

        var price = crypto.CurrentPrice;
        var total = dto.Quantity * price;
        var now = DateTime.UtcNow;

        if (dto.Type == "Buy")
            return await ProcessBuyAsync(userId, dto, price, total, now, token);
        else if (dto.Type == "Sell")
            return await ProcessSellAsync(userId, dto, price, total, now, token);

        return (null!, "Type d'ordre invalide. Utilisez 'Buy' ou 'Sell'.");
    }

    public async Task<List<OrderResponseDto>> GetOrdersAsync(int userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : MapToDto(order);
    }

    public async Task<(bool success, string? error)> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            return (false, "Ordre introuvable.");

        if (order.Status != OrderStatus.Pending)
            return (false, $"Impossible d'annuler un ordre avec le statut '{order.Status}'.");

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        return (true, null);
    }

    private async Task<(OrderResponseDto order, string? error)> ProcessBuyAsync(
        int userId, OrderRequestDto dto, decimal price, decimal total, DateTime now, string token)
    {
        // 2. Vérifier le solde suffisant
        var balance = await _authClient.GetBalanceAsync(token);

        var order = new Order
        {
            UserId = userId,
            CryptoSymbol = dto.CryptoSymbol,
            Type = OrderType.Buy,
            Quantity = dto.Quantity,
            Price = price,
            Total = total,
            CreatedAt = now
        };

        if (balance < total)
        {
            order.Status = OrderStatus.Rejected;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return (MapToDto(order), $"Solde insuffisant. Disponible : {balance}$, requis : {total}$.");
        }

        order.Status = OrderStatus.Executed;
        order.ExecutedAt = now;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _authClient.DeductBalanceAsync(userId, total, token);
        await _portfolioClient.CreateTransactionAsync(new CreateTransactionDto
        {
            UserId = userId,
            CryptoSymbol = dto.CryptoSymbol,
            Type = "Buy",
            Quantity = dto.Quantity,
            PriceAtTime = price,
            Total = total
        }, token);

        return (MapToDto(order), null);
    }

    private async Task<(OrderResponseDto order, string? error)> ProcessSellAsync(
        int userId, OrderRequestDto dto, decimal price, decimal total, DateTime now, string token)
    {
        var holding = await _portfolioClient.GetHoldingAsync(userId, dto.CryptoSymbol, token);

        if (holding == null || holding.Quantity < dto.Quantity)
        {
            var available = holding?.Quantity ?? 0;
            return (null!, $"Quantité insuffisante. Disponible : {available}, demandé : {dto.Quantity}.");
        }

        var order = new Order
        {
            UserId = userId,
            CryptoSymbol = dto.CryptoSymbol,
            Type = OrderType.Sell,
            Quantity = dto.Quantity,
            Price = price,
            Total = total,
            Status = OrderStatus.Executed,
            CreatedAt = now,
            ExecutedAt = now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _authClient.CreditBalanceAsync(userId, total, token);
        await _portfolioClient.CreateTransactionAsync(new CreateTransactionDto
        {
            UserId = userId,
            CryptoSymbol = dto.CryptoSymbol,
            Type = "Sell",
            Quantity = dto.Quantity,
            PriceAtTime = price,
            Total = total
        }, token);

        return (MapToDto(order), null);
    }

    private static OrderResponseDto MapToDto(Order order) => new()
    {
        OrderId = order.Id,
        CryptoSymbol = order.CryptoSymbol,
        Type = order.Type.ToString(),
        Quantity = order.Quantity,
        Price = order.Price,
        Total = order.Total,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        ExecutedAt = order.ExecutedAt
    };
}