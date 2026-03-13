namespace CryptoSim.Blazor.Services
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
