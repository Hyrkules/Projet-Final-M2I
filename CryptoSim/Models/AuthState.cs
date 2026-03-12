namespace CryptoSim.Blazor.Services;

public static class AuthState
{
    public static bool IsAuthenticated { get; set; } = false;
    public static string Token { get; set; } = string.Empty;
    public static string Username { get; set; } = string.Empty;
    public static decimal Balance { get; set; } = 0;
    public static string Role { get; set; } = string.Empty;
    public static bool IsAdmin => Role == "Admin";
}