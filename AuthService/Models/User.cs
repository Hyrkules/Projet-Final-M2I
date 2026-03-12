using System.Data;

namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.User;
        public decimal Balance { get; set; } = 10_000m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
