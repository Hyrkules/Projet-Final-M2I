using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderManagerService _orderService;

    public OrderController(IOrderManagerService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var token = GetCurrentToken();

        var (order, error) = await _orderService.PlaceOrderAsync(userId.Value, request, token);

        if (error != null && order?.Status == "Rejected")
            return BadRequest(new { message = error, order });

        if (error != null)
            return BadRequest(new { message = error });

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var orders = await _orderService.GetOrdersAsync(userId.Value);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(userId.Value, id);
        if (order == null)
            return NotFound(new { message = $"Ordre #{id} introuvable." });

        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (success, error) = await _orderService.CancelOrderAsync(userId.Value, id);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = $"Ordre #{id} annulé." });
    }

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out var id) ? id : null;
    }

    private string GetCurrentToken()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        return authHeader.StartsWith("Bearer ") ? authHeader["Bearer ".Length..] : string.Empty;
    }
}