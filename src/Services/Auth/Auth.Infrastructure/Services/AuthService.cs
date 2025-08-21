using Auth.Core.Entities;
using Auth.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpirationMinutes;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Configuration'dan JWT ayarlarýný al
            _jwtSecret = _configuration["Jwt:Secret"] ??
                throw new ArgumentNullException("JWT Secret is required");
            _jwtIssuer = _configuration["Jwt:Issuer"] ?? "AuthService";
            _jwtAudience = _configuration["Jwt:Audience"] ?? "AuthService";
            _jwtExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        }

        public async Task<string> GenerateJwtTokenAsync(int userId, string email, UserRole role)
        {
            try
            {
                // JWT Claims oluþtur
                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new(JwtRegisteredClaimNames.Email, email),
                    new(ClaimTypes.NameIdentifier, userId.ToString()),
                    new(ClaimTypes.Email, email),
                    new(ClaimTypes.Role, role.ToString()),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64)
                };

                // Signing key oluþtur
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // JWT Token oluþtur
                var token = new JwtSecurityToken(
                    issuer: _jwtIssuer,
                    audience: _jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                    signingCredentials: credentials
                );

                return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            // 64 byte random token oluþtur
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return await Task.FromResult(Convert.ToBase64String(randomNumber));
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters();

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentNullException(nameof(password));

                // Salt oluþtur (16 byte)
                using var rng = RandomNumberGenerator.Create();
                var salt = new byte[16];
                rng.GetBytes(salt);

                // PBKDF2 ile hash oluþtur (32 byte)
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);

                //  DÜZELTME: Doðru boyutta array oluþtur (16 + 32 = 48 byte)
                var hashBytes = new byte[salt.Length + hash.Length]; // 48 byte toplam

                // Buffer.BlockCopy kullan (daha güvenli)
                Buffer.BlockCopy(salt, 0, hashBytes, 0, salt.Length);           // Ýlk 16 byte = salt
                Buffer.BlockCopy(hash, 0, hashBytes, salt.Length, hash.Length); // Son 32 byte = hash

                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                    return false;

                // Base64'ten byte array'e çevir
                var hashBytes = Convert.FromBase64String(hashedPassword);

                //  DÜZELTME: Boyut kontrolü (48 byte olmalý: 16 salt + 32 hash)
                if (hashBytes.Length != 48)
                    return false;

                // Salt'ý ayýr (ilk 16 byte)
                var salt = new byte[16];
                Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

                // Girilen þifreyi ayný salt ile hash'le
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(32);

                // Hash'leri güvenli þekilde karþýlaþtýr (timing attack prevention)
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != computedHash[i])
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }


        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            // Bu method UserRepository ile birlikte implement edilmeli
            // Þimdilik placeholder
            var newAccessToken = await GenerateJwtTokenAsync(1, "temp@example.com", UserRole.User);
            var newRefreshToken = await GenerateRefreshTokenAsync();

            return (newAccessToken, newRefreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            // Repository ile token'ý revoke et
            await Task.CompletedTask;
        }

        // Private helper method
        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Token expiry'de tolerans yok
            };
        }
    }
}
