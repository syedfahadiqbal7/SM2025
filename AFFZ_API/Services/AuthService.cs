using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AFFZ_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateJwtTokenAsync(string userId, string role, string email)
        {
            try
            {
                var jwtSecret = _configuration["Security:JwtSecretKey"];
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    throw new InvalidOperationException("JWT secret key not configured");
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSecret);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(24),
                    Issuer = _configuration["Security:JwtIssuer"] ?? "SmartCenter",
                    Audience = _configuration["Security:JwtAudience"] ?? "SmartCenterUsers",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtSecret = _configuration["Security:JwtSecretKey"];
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogError("JWT secret key not configured");
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Security:JwtIssuer"] ?? "SmartCenter",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Security:JwtAudience"] ?? "SmartCenterUsers",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                // In a real application, you would add the token to a blacklist
                // For now, we'll just return true as the token will expire naturally
                _logger.LogInformation("Token revocation requested for token: {Token}", token.Substring(0, Math.Min(20, token.Length)) + "...");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking JWT token");
                return false;
            }
        }
    }
}
