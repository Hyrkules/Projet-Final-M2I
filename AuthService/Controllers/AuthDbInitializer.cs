using AuthService.Models;
using Projet_CryptoSim.AuthService.Data;

namespace AuthService.Controllers
{
    public class AuthDbInitializer
    {
        public static void Initialize(AuthDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any()) return;

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@cryptosim.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
                Role = Role.Admin,
                Balance = 10_000m,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}
