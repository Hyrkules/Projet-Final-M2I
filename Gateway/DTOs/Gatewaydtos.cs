using System.ComponentModel.DataAnnotations;

namespace Gateway.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public long ExpiresIn { get; set; }
}

// ── Market ────────────────────────────────────────────────────────────────────

public class CryptoDto
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PriceHistoryDto
{
    public string CryptoSymbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime RecordedAt { get; set; }
}

// ── Orders ────────────────────────────────────────────────────────────────────

public class OrderRequestDto
{
    [Required]
    public string CryptoSymbol { get; set; } = string.Empty;
    [Required]
    public string Type { get; set; } = string.Empty;
    [Range(0.00000001, double.MaxValue)]
    public decimal Quantity { get; set; }
}

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string CryptoSymbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
}

// ── Portfolio ─────────────────────────────────────────────────────────────────

public class PortfolioSummaryDto
{
    public int UserId { get; set; }
    public decimal CashBalance { get; set; }
    public decimal TotalInvested { get; set; }
    public decimal TotalCurrentValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPercent { get; set; }
    public List<HoldingDetailDto> Holdings { get; set; } = new();
}

public class HoldingDetailDto
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageBuyPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal GainLoss { get; set; }
    public decimal GainLossPercent { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public string CryptoSymbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PriceAtTime { get; set; }
    public decimal Total { get; set; }
    public DateTime ExecutedAt { get; set; }
}