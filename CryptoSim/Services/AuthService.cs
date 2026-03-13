namespace CryptoSim.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponse?> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });

        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> RegisterAsync(string username, string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email,
            password
        });

        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }
}