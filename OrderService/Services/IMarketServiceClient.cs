using static OrderService.DTOs.ExternalDTO;

namespace OrderService.Services
{
    public interface IMarketServiceClient
    {
        Task<CryptoPrice?> GetCryptoAsync(string symbol);
    }
}
