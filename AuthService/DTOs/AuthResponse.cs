namespace AuthService.DTOs
{
    public class AuthResponse
    {
        public string Token;
        public string Username;
        public string Role;
        public decimal Balance;
        public long ExpiresIn;
    }
}
