using Gateway.DTOs;
using Gateway.Filters;
using Gateway.RestClient;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/orders")]
[RequireValidToken]
public class OrderController : ControllerBase
{
    private readonly string _orderBaseUrl;

    public OrderController(IConfiguration config)
    {
        _orderBaseUrl = config["Services:OrderService"] ?? "http://localhost:5004";
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDto dto)
    {
        var token = GetToken();
        var client = new Client<OrderResponseDto, OrderRequestDto>(_orderBaseUrl, token);
        var result = await client.PostRequest("/api/orders", dto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var token = GetToken();
        var client = new Client<OrderResponseDto, object>(_orderBaseUrl, token);
        var result = await client.GetRequestList("/api/orders");
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var token = GetToken();
        var client = new Client<OrderResponseDto, object>(_orderBaseUrl, token);
        var result = await client.GetRequest($"/api/orders/{id}");
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var token = GetToken();
        var client = new Client<object, object>(_orderBaseUrl, token);
        var result = await client.DeleteRequest($"/api/orders/{id}");
        return Ok(result);
    }

    private string GetToken()
    {
        var header = Request.Headers["Authorization"].ToString();
        return header.StartsWith("Bearer ") ? header["Bearer ".Length..] : header;
    }
}