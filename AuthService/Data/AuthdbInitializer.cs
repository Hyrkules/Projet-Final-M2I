using AuthService.Models;
using Projet_CryptoSim.AuthService.Data;

namespace AuthService.Data
{
    public class AuthdbInitializer
    {
        public static void Initialize(AuthdbContext context)
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
