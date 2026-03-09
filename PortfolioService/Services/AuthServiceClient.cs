namespace PortfolioService.Services
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var response = await _httpClient.GetAsync("/api/auth/balance");
            if (!response.IsSuccessStatusCode) return 0;

            var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            return result?.Balance ?? 0;
        }

        private class BalanceResponse
        {
            public decimal Balance { get; set; }
        }
    }
}
