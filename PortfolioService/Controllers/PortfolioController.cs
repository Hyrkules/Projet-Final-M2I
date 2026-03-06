using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioService.DTOs;
using PortfolioService.Services;
using System.Security.Claims;

namespace PortfolioService.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioManagerService _portfolioService;

    public PortfolioController(IPortfolioManagerService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    // GET /api/portfolio
    // Portefeuille complet avec valeur actuelle
    [HttpGet]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var performance = await _portfolioService.GetSummaryAsync(userId.Value);
        return Ok(performance);
    }

    // GET /api/portfolio/holdings
    // Liste des cryptos détenues
    [HttpGet("holdings")]
    public async Task<IActionResult> GetHoldings()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var holdings = await _portfolioService.GetHoldingsAsync(userId.Value);
        return Ok(holdings);
    }

    // GET /api/portfolio/holdings/{symbol}
    // Holding d'une crypto spécifique
    [HttpGet("holdings/{symbol}")]
    public async Task<IActionResult> GetHolding(string symbol)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var holding = await _portfolioService.GetHoldingAsync(userId.Value, symbol);
        if (holding == null)
            return NotFound(new { message = $"Vous ne détenez pas de {symbol}." });

        return Ok(holding);
    }

    // GET /api/portfolio/transactions
    // Historique des transactions
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var transactions = await _portfolioService.GetTransactionsAsync(userId.Value);
        return Ok(transactions);
    }

    // GET /api/portfolio/summary
    // Résumé de performance du portefeuille
    [HttpGet("performance")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var performance = await _portfolioService.GetSummaryAsync(userId.Value);
        return Ok(performance);
    }

    // POST /api/portfolio/transactions
    // Créer une transaction — appelé par OrderService
    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        var transaction = await _portfolioService.CreateTransactionAsync(dto);
        return CreatedAtAction(nameof(GetTransactions), transaction);
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out var id) ? id : null;
    }
}