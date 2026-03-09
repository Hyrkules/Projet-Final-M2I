namespace PortfolioService.Services
{
    public interface IMarketServiceClient
    {
        Task<CryptoPrice?> GetCryptoAsync(string symbol);
    }
}
