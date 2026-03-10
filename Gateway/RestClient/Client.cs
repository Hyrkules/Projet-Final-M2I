using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gateway.RestClient;

public class Client<TResponse, TRequest>
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    // Options JSON pour correspondre au format des microservices (camelCase)
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public Client(string baseUrl, string? token = null)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();

        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    public async Task<TResponse> GetRequest(string url)
    {
        var response = await _httpClient.GetAsync(_baseUrl + url);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erreur {(int)response.StatusCode} sur GET {url}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TResponse>(json, JsonOptions);
        return result ?? throw new Exception("Réponse nulle.");
    }

    public async Task<List<TResponse>> GetRequestList(string url)
    {
        var response = await _httpClient.GetAsync(_baseUrl + url);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erreur {(int)response.StatusCode} sur GET {url}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<TResponse>>(json, JsonOptions);
        return result ?? throw new Exception("Réponse nulle.");
    }

    public async Task<TResponse> PostRequest(string url, TRequest body)
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(_baseUrl + url, content);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erreur {(int)response.StatusCode} sur POST {url} : {json}");

        var result = JsonSerializer.Deserialize<TResponse>(json, JsonOptions);
        return result ?? throw new Exception("Réponse nulle.");
    }

    public async Task<TResponse> DeleteRequest(string url)
    {
        var response = await _httpClient.DeleteAsync(_baseUrl + url);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erreur {(int)response.StatusCode} sur DELETE {url}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TResponse>(json, JsonOptions);
        return result ?? throw new Exception("Réponse nulle.");
    }
}