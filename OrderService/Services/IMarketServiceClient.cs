namespace OrderService.Services
{
    public interface IMarketServiceClient
    {
        Task<CryptoPrice?> GetCryptoAsync(string symbol);
    }
}
