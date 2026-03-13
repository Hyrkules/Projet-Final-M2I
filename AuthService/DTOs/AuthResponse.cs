namespace AuthService.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public long ExpiresIn { get; set; }
    }
}
