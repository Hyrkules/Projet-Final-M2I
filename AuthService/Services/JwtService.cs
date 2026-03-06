using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projet_CryptoSim.AuthService.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Articles.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        TokenValidationParameters GetValidationParameters();
    }

    public class JwtService : IJwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
            _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
            _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
            _expirationMinutes = int.Parse(config["Jwt:ExpirationMinutes"] ?? "1440");
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(_expirationMinutes);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                      new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                      ClaimValueTypes.Integer64)
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenValidationParameters GetValidationParameters() => new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}