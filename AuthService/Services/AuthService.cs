using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Identity.Data;
using Projet_CryptoSim.AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        private readonly AuthDbContext _context;

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
            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (existingUsername != null)
                throw new InvalidOperationException("Ce nom d'utilisateur est déjà pris.");

            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException("Cette adresse e-mail est déjà utilisée.");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordService.HashPassword(dto.Password),
                Email = dto.Email,
                Role = Role.User,
                Balance = 10_000m,  
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

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !PasswordService.VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = PasswordService.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUsernameAsync(int userId, string newUsername, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !PasswordService.VerifyPassword(password, user.PasswordHash))
                return false;

            var exists = await _context.Users.AnyAsync(u => u.Username == newUsername && u.Id != userId);
            if (exists) return false;

            user.Username = newUsername;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}