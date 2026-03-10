using CryptoSim.Blazor.Services;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Net.Http.Headers;

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
                    o.ExecutedAt >= DateTime.UtcNow.AddDays(-_periodFilter));

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

            var result = await Http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
            _orders = result ?? new();
        }
        catch { _orders = new(); }
        finally { _isLoading = false; }
    }

    private class OrderDto
    {
        public string CryptoSymbol { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExecutedAt { get; set; }
    }
}