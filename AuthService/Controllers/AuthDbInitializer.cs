using AuthService.Models;
using Projet_CryptoSim.AuthService.Data;

namespace AuthService.Controllers
{
    public class AuthDbInitializer
    {
        public static void Initialize(AuthDbContext context)
        {
            // Applique les migrations en attente si la base n'existe pas encore
            context.Database.EnsureCreated();

            // Si des utilisateurs existent déjà, on ne fait rien
            if (context.Users.Any()) return;

            // Seed : un compte Admin par défaut
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
