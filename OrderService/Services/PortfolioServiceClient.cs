namespace OrderService.Services
{
    public class PortfolioServiceClient : IPortfolioServiceClient
    {
        private readonly HttpClient _httpClient;

        public PortfolioServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HoldingInfo?> GetHoldingAsync(int userId, string symbol, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/portfolio/holdings/{symbol}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<HoldingInfo>();
        }

        public async Task<bool> CreateTransactionAsync(CreateTransactionDto dto, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/portfolio/transactions");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = JsonContent.Create(dto);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
