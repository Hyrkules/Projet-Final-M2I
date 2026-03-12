using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponse> LoginAsync(LoginRequestDto dto);
        Task<User?> CreditBalanceAsync(int userId, decimal amount);
        Task<User?> DeductBalanceAsync(int userId, decimal amount);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ChangeUsernameAsync(int userId, string newUsername, string password);
    }
}
