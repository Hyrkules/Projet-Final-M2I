using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projet_CryptoSim.AuthService.Data;
using System.Security.Claims;

namespace Projet_CryptoSim.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthDbContext _db;

    public AuthController(IAuthService authService, AuthDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Me), response);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var user = await _db.Users.FindAsync(userId.Value);
        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Role = user.Role.ToString(),
            user.Balance,
            user.CreatedAt
        });
    }

    // GET /api/auth/balance
    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> Balance()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var user = await _db.Users.FindAsync(userId.Value);
        if (user is null) return NotFound();

        return Ok(new { user.Id, user.Balance });
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out var id) ? id : null;
    }
}