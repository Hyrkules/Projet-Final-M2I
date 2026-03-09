namespace PortfolioService.Services
{
    public interface IAuthServiceClient
    {
        Task<decimal> GetBalanceAsync();
    }
}
