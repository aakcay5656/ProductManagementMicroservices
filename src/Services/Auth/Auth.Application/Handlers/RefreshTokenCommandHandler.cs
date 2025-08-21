using Auth.Application.DTOs;
using Auth.Application.Features.Commands;
using Auth.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;

namespace Auth.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IUnitOfWork unitOfWork,
            IAuthService authService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing token refresh");

                // 1. Refresh token ile user bul
                var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);
                if (user == null)
                {
                    return Result<AuthResponseDto>.Failure("Invalid refresh token");
                }

                // 2. Account status kontrol
                if (!user.IsActive || user.IsDeleted)
                {
                    return Result<AuthResponseDto>.Failure("Account is deactivated");
                }

                // 3. Transaction başlat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 4. Yeni token'lar oluştur (Token Rotation)
                    var newAccessToken = await _authService.GenerateJwtTokenAsync(
                        user.Id,
                        user.Email,
                        user.Role
                    );
                    var newRefreshToken = await _authService.GenerateRefreshTokenAsync();

                    // 5. User'daki refresh token'ı güncelle
                    user.RefreshToken = newRefreshToken;
                    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var response = new AuthResponseDto
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role.ToString(),
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                    };

                    _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Success(response);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return Result<AuthResponseDto>.Failure("Token refresh failed");
            }
        }
    }
}
