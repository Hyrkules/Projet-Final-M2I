using Gateway.DTOs;
using Gateway.Filters;
using Gateway.RestClient;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly string _authBaseUrl;

    public AuthController(IConfiguration config)
    {
        _authBaseUrl = config["Services:AuthService"] ?? "http://localhost:5001";
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var client = new Client<AuthResponseDto, RegisterRequestDto>(_authBaseUrl);
        var result = await client.PostRequest("/api/auth/register", dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var client = new Client<AuthResponseDto, LoginRequestDto>(_authBaseUrl);
        var result = await client.PostRequest("/api/auth/login", dto);
        return Ok(result);
    }

    [HttpGet("me")]
    [RequireValidToken]
    public async Task<IActionResult> Me()
    {
        var token = GetToken();
        var client = new Client<object, object>(_authBaseUrl, token);
        var result = await client.GetRequest("/api/auth/me");
        return Ok(result);
    }

    [HttpGet("balance")]
    [RequireValidToken]
    public async Task<IActionResult> Balance()
    {
        var token = GetToken();
        var client = new Client<object, object>(_authBaseUrl, token);
        var result = await client.GetRequest("/api/auth/balance");
        return Ok(result);
    }

    private string GetToken()
    {
        var header = Request.Headers["Authorization"].ToString();
        return header.StartsWith("Bearer ") ? header["Bearer ".Length..] : header;
    }
}