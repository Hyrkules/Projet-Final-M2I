using static OrderService.DTOs.ExternalDTO;

namespace OrderService.Services
{
    public interface IPortfolioServiceClient
    {
        Task<HoldingInfo?> GetHoldingAsync(int userId, string symbol, string token);
        Task<bool> CreateTransactionAsync(CreateTransactionDto dto, string token);
    }
}
