namespace OrderService.Services
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetBalanceAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/balance");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return 0;

            var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            return result?.Balance ?? 0;
        }

        public async Task DeductBalanceAsync(int userId, decimal amount, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/balance/deduct");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = JsonContent.Create(new { userId, amount });

            await _httpClient.SendAsync(request);
        }

        public async Task CreditBalanceAsync(int userId, decimal amount, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/balance/credit");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = JsonContent.Create(new { userId, amount });

            await _httpClient.SendAsync(request);
        }

        private class BalanceResponse
        {
            public decimal Balance { get; set; }
        }
    }
}
