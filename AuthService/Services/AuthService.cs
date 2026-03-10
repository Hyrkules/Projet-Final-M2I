using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Identity.Data;
using Projet_CryptoSim.AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        // IConfiguration => accès à appsettings.json
        // Injecté automatiquement par ASP.NET via le constructeur
        private readonly IConfiguration _config;

        // DbContext pour accéder à la base users_db
        private readonly AuthDbContext _context;

        // JwtService pour générer les tokens
        private readonly IJwtService _jwtService;

        public AuthService(IConfiguration config, AuthDbContext context, IJwtService jwtService)
        {
            _config = config;
            _context = context;
            _jwtService = jwtService;
        }

        // Inscription : crée un compte et retourne un JWT
        public async Task<AuthResponse> RegisterAsync(RegisterRequestDto dto)
        {
            // Vérification unicité du username
            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (existingUsername != null)
                throw new InvalidOperationException("Ce nom d'utilisateur est déjà pris.");

            // Vérification unicité de l'email
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException("Cette adresse e-mail est déjà utilisée.");

            // Création de l'utilisateur avec mot de passe hashé
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordService.HashPassword(dto.Password),
                Email = dto.Email,
                Role = Role.User,
                Balance = 10_000m,  // solde virtuel de départ
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        // Connexion : vérifie les credentials et retourne un JWT
        public async Task<AuthResponse> LoginAsync(LoginRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            // Si l'utilisateur n'existe pas ou le mot de passe est incorrect
            if (user == null || !PasswordService.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Identifiants incorrects.");

            return BuildAuthResponse(user);
        }

        public async Task<User?> CreditBalanceAsync(int userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;
            user.Balance += amount;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> DeductBalanceAsync(int userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Balance < amount) return null;
            user.Balance -= amount;
            await _context.SaveChangesAsync();
            return user;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private AuthResponse BuildAuthResponse(User user)
        {
            var token = _jwtService.GenerateToken(user);

            return new AuthResponse{
                Token = token,
                Username = user.Username,
                Role = user.Role.ToString(),
                Balance = user.Balance,
                ExpiresIn = 1440 * 60  // 24h en secondes
        };
        }
    }
}