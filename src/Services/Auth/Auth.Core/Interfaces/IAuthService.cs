using Auth.Core.Entities;

namespace Auth.Core.Interfaces
{
    public interface IAuthService
    {
        // JWT Token Operasyonları
        Task<string> GenerateJwtTokenAsync(int userId, string email, UserRole role);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateTokenAsync(string token);

        // Password Operasyonları
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);

        // Refresh Token Operasyonları
        Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
