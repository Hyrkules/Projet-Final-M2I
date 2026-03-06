using AuthService.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponse> LoginAsync(LoginRequestDto dto);
    }
}
