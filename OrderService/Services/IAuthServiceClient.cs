namespace OrderService.Services
{
    public interface IAuthServiceClient
    {
        Task<decimal> GetBalanceAsync(string token);
        Task DeductBalanceAsync(int userId, decimal amount, string token);
        Task CreditBalanceAsync(int userId, decimal amount, string token);
    }

}
