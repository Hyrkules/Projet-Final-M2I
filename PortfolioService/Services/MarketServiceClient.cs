namespace PortfolioService.Services
{
    public class MarketServiceClient : IMarketServiceClient
    {
        private readonly HttpClient _httpClient;

        public MarketServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CryptoPrice?> GetCryptoAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"/api/market/cryptos/{symbol}");
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<CryptoPrice>();
        }
    }
}
