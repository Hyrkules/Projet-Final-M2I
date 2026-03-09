namespace OrderService.Services
{
    public interface IAuthServiceClient
    {
        Task<decimal> GetBalanceAsync(string token);
        Task<bool> DeductBalanceAsync(int userId, decimal amount, string token);
        Task<bool> CreditBalanceAsync(int userId, decimal amount, string token);
    }
}
