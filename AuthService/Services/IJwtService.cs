using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        TokenValidationParameters GetValidationParameters();
    }
}
