using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.DTOs;
using PortfolioService.Models;
using System.Transactions;

namespace PortfolioService.Services;

public class PortfolioManagerService : IPortfolioManagerService
{
    private readonly PortfolioDbContext _context;
    private readonly IMarketServiceClient _marketClient;

    public PortfolioManagerService(PortfolioDbContext context, IMarketServiceClient marketClient)
    {
        _context = context;
        _marketClient = marketClient;
    }

    // Résumé complet du portefeuille avec valeur actuelle
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

    // Liste des holdings enrichis avec les prix actuels depuis MarketService
    public async Task<List<HoldingDto>> GetHoldingsAsync(int userId)
    {
        var holdings = await _context.Holdings
            .Where(h => h.UserId == userId && h.Quantity > 0)
            .ToListAsync();

        var result = new List<HoldingDto>();

        foreach (var holding in holdings)
        {
            // Appel HTTP vers MarketService pour le prix actuel
            var crypto = await _marketClient.GetCryptoAsync(holding.CryptoSymbol);
            var currentPrice = crypto?.CurrentPrice ?? holding.AverageBuyPrice;

            var currentValue = holding.Quantity * currentPrice;
            var invested = holding.Quantity * holding.AverageBuyPrice;
            var profitLoss = currentValue - invested;
            var profitLossPct = invested == 0 ? 0 : Math.Round(profitLoss / invested * 100, 2);

            result.Add(new HoldingDto
            {
                CryptoSymbol = holding.CryptoSymbol,
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

    // Historique des transactions d'un utilisateur
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

    // Holding d'un utilisateur pour une crypto spécifique
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

    // Créer une transaction + mettre à jour le holding (appelé par OrderService)
    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto)
    {
        var now = DateTime.UtcNow;

        // 1. Enregistrer la transaction
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

        // 2. Mettre à jour le holding
        var holding = await _context.Holdings
            .FirstOrDefaultAsync(h => h.UserId == dto.UserId
                && h.CryptoSymbol == dto.CryptoSymbol);

        if (dto.Type == "Buy")
        {
            if (holding == null)
            {
                // Premier achat : créer le holding
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
                // Achat supplémentaire : recalculer le prix moyen
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