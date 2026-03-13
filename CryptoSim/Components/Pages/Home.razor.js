[JSInvokable]
public async Task < object > GetMarketStats(string symbol)
{
    try {
        // On appelle ton controller qui donne le snapshot/stats
        var crypto = await Http.GetFromJsonAsync < CryptoDto > ($"/api/market/cryptos/{symbol}");
        if (crypto == null) return new { high = 0, low = 0, change = 0 };

        return new
            {
                high = crypto.CurrentPrice, // Si ton API n'a pas encore de High24h, on met le prix actuel
                low = crypto.CurrentPrice,
                change = 0.0 // À calculer si tu as le prix d'il y a 24h
            };
    }
    catch { return new { high = 0, low = 0, change = 0 }; }
}