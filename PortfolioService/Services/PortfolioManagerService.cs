using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.DTOs;
using PortfolioService.Models;

namespace PortfolioService.Services;

public class PortfolioManagerService : IPortfolioManagerService
{
    private readonly PortfoliodbContext _context;
    private readonly IMarketServiceClient _marketClient;

    public PortfolioManagerService(PortfoliodbContext context, IMarketServiceClient marketClient)
    {
        _context = context;
        _marketClient = marketClient;
    }

    public async Task<PortfolioSummaryDto> GetSummaryAsync(int userId)
    {
        var holdings = await GetHoldingsAsync(userId);

        var totalInvested = holdings.Sum(h => h.Quantity * h.AverageBuyPrice);
        var totalValue = holdings.Sum(h => h.CurrentValue);
        var totalProfitLoss = totalValue - totalInvested;
        var profitLossPercent = totalInvested == 0 ? 0
            : Math.Round(totalProfitLoss / totalInvested * 100, 2);

        return new PortfolioSummaryDto
        {
            UserId = userId,
            TotalInvested = Math.Round(totalInvested, 2),
            TotalValue = Math.Round(totalValue, 2),
            TotalProfitLoss = Math.Round(totalProfitLoss, 2),
            ProfitLossPercent = profitLossPercent,
            Holdings = holdings
        };
    }

    public async Task<List<HoldingDto>> GetHoldingsAsync(int userId)
    {
        var holdings = await _context.Holdings
            .Where(h => h.UserId == userId && h.Quantity > 0)
            .ToListAsync();

        var result = new List<HoldingDto>();

        foreach (var holding in holdings)
        {
            var crypto = await _marketClient.GetCryptoAsync(holding.CryptoSymbol);
            var currentPrice = crypto?.CurrentPrice ?? holding.AverageBuyPrice;

            var currentValue = holding.Quantity * currentPrice;
            var invested = holding.Quantity * holding.AverageBuyPrice;
            var profitLoss = currentValue - invested;
            var profitLossPct = invested == 0 ? 0 : Math.Round(profitLoss / invested * 100, 2);

            result.Add(new HoldingDto
            {
                CryptoSymbol = holding.CryptoSymbol,
                CryptoName = crypto?.Name ?? holding.CryptoSymbol,
                Quantity = holding.Quantity,
                AverageBuyPrice = holding.AverageBuyPrice,
                CurrentPrice = currentPrice,
                CurrentValue = Math.Round(currentValue, 2),
                ProfitLoss = Math.Round(profitLoss, 2),
                ProfitLossPercent = profitLossPct,
                UpdatedAt = holding.UpdatedAt
            });
        }

        return result;
    }

    public async Task<List<TransactionDto>> GetTransactionsAsync(int userId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync();

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            CryptoSymbol = t.CryptoSymbol,
            Type = t.Type,
            Quantity = t.Quantity,
            PriceAtTime = t.PriceAtTime,
            Total = t.Total,
            ExecutedAt = t.ExecutedAt
        }).ToList();
    }

    public async Task<HoldingDto?> GetHoldingAsync(int userId, string symbol)
    {
        var holding = await _context.Holdings
            .FirstOrDefaultAsync(h => h.UserId == userId
                && h.CryptoSymbol.ToUpper() == symbol.ToUpper());

        if (holding == null) return null;

        var crypto = await _marketClient.GetCryptoAsync(holding.CryptoSymbol);
        var currentPrice = crypto?.CurrentPrice ?? holding.AverageBuyPrice;

        var currentValue = holding.Quantity * currentPrice;
        var invested = holding.Quantity * holding.AverageBuyPrice;
        var profitLoss = currentValue - invested;
        var profitLossPct = invested == 0 ? 0 : Math.Round(profitLoss / invested * 100, 2);

        return new HoldingDto
        {
            CryptoSymbol = holding.CryptoSymbol,
            Quantity = holding.Quantity,
            AverageBuyPrice = holding.AverageBuyPrice,
            CurrentPrice = currentPrice,
            CurrentValue = Math.Round(currentValue, 2),
            ProfitLoss = Math.Round(profitLoss, 2),
            ProfitLossPercent = profitLossPct,
            UpdatedAt = holding.UpdatedAt
        };
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto)
    {
        var now = DateTime.UtcNow;

        var transaction = new Models.Transaction
        {
            UserId = dto.UserId,
            CryptoSymbol = dto.CryptoSymbol,
            Type = dto.Type,
            Quantity = dto.Quantity,
            PriceAtTime = dto.PriceAtTime,
            Total = dto.Total,
            ExecutedAt = now
        };

        _context.Transactions.Add(transaction);

        var holding = await _context.Holdings
            .FirstOrDefaultAsync(h => h.UserId == dto.UserId
                && h.CryptoSymbol == dto.CryptoSymbol);

        if (dto.Type == "Buy")
        {
            if (holding == null)
            {
                _context.Holdings.Add(new Holding
                {
                    UserId = dto.UserId,
                    CryptoSymbol = dto.CryptoSymbol,
                    Quantity = dto.Quantity,
                    AverageBuyPrice = dto.PriceAtTime,
                    UpdatedAt = now
                });
            }
            else
            {
                var totalQuantity = holding.Quantity + dto.Quantity;
                var totalCost = (holding.Quantity * holding.AverageBuyPrice) + dto.Total;
                holding.AverageBuyPrice = totalCost / totalQuantity;
                holding.Quantity = totalQuantity;
                holding.UpdatedAt = now;
            }
        }
        else if (dto.Type == "Sell" && holding != null)
        {
            holding.Quantity -= dto.Quantity;
            holding.UpdatedAt = now;

            if (holding.Quantity <= 0)
                _context.Holdings.Remove(holding);
        }

        await _context.SaveChangesAsync();

        return new TransactionDto
        {
            Id = transaction.Id,
            CryptoSymbol = transaction.CryptoSymbol,
            Type = transaction.Type,
            Quantity = transaction.Quantity,
            PriceAtTime = transaction.PriceAtTime,
            Total = transaction.Total,
            ExecutedAt = transaction.ExecutedAt
        };
    }
}