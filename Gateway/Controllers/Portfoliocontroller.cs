using Gateway.DTOs;
using Gateway.Filters;
using Gateway.RestClient;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/portfolio")]
[RequireValidToken]
public class PortfolioController : ControllerBase
{
    private readonly string _portfolioBaseUrl;

    public PortfolioController(IConfiguration config)
    {
        _portfolioBaseUrl = config["Services:PortfolioService"] ?? "http://localhost:5003";
    }

    // GET /api/portfolio
    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var token = GetToken();
        var client = new Client<PortfolioSummaryDto, object>(_portfolioBaseUrl, token);
        var result = await client.GetRequest("/api/portfolio");
        return Ok(result);
    }

    // GET /api/portfolio/holdings
    [HttpGet("holdings")]
    public async Task<IActionResult> GetHoldings()
    {
        var token = GetToken();
        var client = new Client<HoldingDetailDto, object>(_portfolioBaseUrl, token);
        var result = await client.GetRequestList("/api/portfolio/holdings");
        return Ok(result);
    }

    // GET /api/portfolio/transactions
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var token = GetToken();
        var client = new Client<TransactionDto, object>(_portfolioBaseUrl, token);
        var result = await client.GetRequestList("/api/portfolio/transactions");
        return Ok(result);
    }

    // GET /api/portfolio/performance
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformance()
    {
        var token = GetToken();
        var client = new Client<PortfolioSummaryDto, object>(_portfolioBaseUrl, token);
        var result = await client.GetRequest("/api/portfolio/performance");
        return Ok(result);
    }

    private string GetToken()
    {
        var header = Request.Headers["Authorization"].ToString();
        return header.StartsWith("Bearer ") ? header["Bearer ".Length..] : header;
    }
}