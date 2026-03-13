namespace CryptoSim.Blazor.Services;

public static class AuthState
{
    public static bool IsAuthenticated { get; set; } = false;
    public static string Token { get; set; } = string.Empty;
    public static string Email { get; set; } = string.Empty;
    public static string Username { get; set; } = string.Empty;
    public static string Role { get; set; } = string.Empty;
    public static bool IsAdmin => Role == "Admin";
    public static DateTime CreatedAt { get; set; } = DateTime.MinValue;

    private static decimal _balance = 0;
    public static decimal Balance
    {
        get => _balance;
        set
        {
            _balance = value;
            OnBalanceChanged?.Invoke();
        }
    }

    public static event Action? OnBalanceChanged;
}