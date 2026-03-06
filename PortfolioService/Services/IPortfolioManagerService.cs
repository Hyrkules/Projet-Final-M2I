using PortfolioService.DTOs;

namespace PortfolioService.Services;

public interface IPortfolioManagerService
{
    Task<PortfolioSummaryDto> GetSummaryAsync(int userId);
    Task<List<HoldingDto>> GetHoldingsAsync(int userId);
    Task<List<TransactionDto>> GetTransactionsAsync(int userId);
    Task<HoldingDto?> GetHoldingAsync(int userId, string symbol);
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto);
}