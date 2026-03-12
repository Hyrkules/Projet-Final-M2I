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

    [Authorize]
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Me), response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

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

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out var id) ? id : null;
    }

    [HttpPost("balance/credit")]
    [Authorize]
    public async Task<IActionResult> CreditBalance([FromBody] BalanceUpdateDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _authService.CreditBalanceAsync(userId, dto.Amount);
        if (user == null) return NotFound();
        return Ok(new { balance = user.Balance });
    }

    [HttpPost("balance/deduct")]
    [Authorize]
    public async Task<IActionResult> DeductBalance([FromBody] BalanceUpdateDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _authService.DeductBalanceAsync(userId, dto.Amount);
        if (user == null) return BadRequest(new { error = "Solde insuffisant." });
        return Ok(new { balance = user.Balance });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var success = await _authService.ChangePasswordAsync(userId.Value, dto.CurrentPassword, dto.NewPassword);
        if (!success) return BadRequest(new { message = "Mot de passe actuel incorrect." });

        return Ok(new { message = "Mot de passe modifié avec succès." });
    }

    [Authorize]
    [HttpPost("change-username")]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var success = await _authService.ChangeUsernameAsync(userId.Value, dto.NewUsername, dto.Password);
        if (!success) return BadRequest(new { message = "Mot de passe incorrect ou nom d'utilisateur déjà pris." });

        return Ok(new { message = "Nom d'utilisateur modifié avec succès." });
    }
}