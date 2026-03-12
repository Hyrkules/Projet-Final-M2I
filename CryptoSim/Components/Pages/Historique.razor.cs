using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CryptoSim.Blazor.Components.Pages;

public partial class Historique
{

    private List<OrderDto> _orders = new();
    private bool _isLoading = true;
    private string _search = string.Empty;
    private string _typeFilter = "all";
    private int _periodFilter = 0;

    private IEnumerable<OrderDto> FilteredOrders => _orders
    .Where(o => string.IsNullOrEmpty(_search) ||
                o.CryptoSymbol.Contains(_search, StringComparison.OrdinalIgnoreCase))
    .Where(o => _typeFilter == "all" || o.Type == _typeFilter)
    .Where(o => _periodFilter == 0 ||
                o.CreatedAt >= DateTime.UtcNow.AddDays(-_periodFilter));

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!AuthState.IsAuthenticated)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            await LoadOrdersAsync();
            StateHasChanged();
        }
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthState.Token);

            var raw = await Http.GetStringAsync("/api/orders");
            Console.WriteLine($">>> Orders JSON: {raw}"); // ✅ on voit le vrai nom du champ

            var result = System.Text.Json.JsonSerializer.Deserialize<List<OrderDto>>(raw,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _orders = result ?? new();
        }
        catch (Exception ex) { Console.WriteLine($">>> Erreur: {ex.Message}"); _orders = new(); }
        finally { _isLoading = false; }
    }

    private static DateTime ToParisTime(DateTime utcDate)
    {
        var parisZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, parisZone);
    }

    private class OrderDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}