using Auth.Application.DTOs;
using Auth.Application.Features.Commands;
using Auth.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;

namespace Auth.Application.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            IAuthService authService,
            ILogger<LoginCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing login for email: {Email}", request.Email);

                // 1. User'ı bul
                var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // 2. Account durumu kontrol
                if (!user.IsActive || user.IsDeleted)
                {
                    return Result<AuthResponseDto>.Failure("Account is deactivated");
                }

                // 3. Password kontrol
                if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }

                // 4. Transaction başlat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 5. Last login güncelle ve yeni token'lar oluştur
                    user.LastLoginAt = DateTime.UtcNow;

                    var accessToken = await _authService.GenerateJwtTokenAsync(
                        user.Id,
                        user.Email,
                        user.Role
                    );
                    var refreshToken = await _authService.GenerateRefreshTokenAsync();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var response = new AuthResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role.ToString(),
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                    };

                    _logger.LogInformation("User logged in successfully: {UserId}", user.Id);
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
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure("Login failed");
            }
        }
    }
}
