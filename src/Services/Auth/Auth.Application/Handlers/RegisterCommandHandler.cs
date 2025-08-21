using Auth.Application.DTOs;
using Auth.Application.Features.Commands;
using Auth.Core.Entities;
using Auth.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;

namespace Auth.Application.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IAuthService authService,
            ILogger<RegisterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing user registration for email: {Email}", request.Email);

                // 1. Email zaten var mý?
                if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
                {
                    return Result<AuthResponseDto>.Failure("Email already exists");
                }

                // 2. Transaction baþlat (ACID properties için)
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 3. User oluþtur
                    var user = new User
                    {
                        Email = request.Email,
                        PasswordHash = _authService.HashPassword(request.Password),
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Role = UserRole.User,
                        IsActive = true
                    };

                    // 4. User'ý kaydet
                    var createdUser = await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    // 5. JWT ve Refresh token oluþtur
                    var accessToken = await _authService.GenerateJwtTokenAsync(
                        createdUser.Id,
                        createdUser.Email,
                        createdUser.Role
                    );
                    var refreshToken = await _authService.GenerateRefreshTokenAsync();

                    // 6. Refresh token'ý user'a kaydet
                    createdUser.RefreshToken = refreshToken;
                    createdUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
                    await _unitOfWork.Users.UpdateAsync(createdUser);
                    await _unitOfWork.SaveChangesAsync();

                    // 7. Transaction'ý commit et
                    await _unitOfWork.CommitTransactionAsync();

                    // 8. Response oluþtur
                    var response = new AuthResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        Email = createdUser.Email,
                        FirstName = createdUser.FirstName,
                        LastName = createdUser.LastName,
                        Role = createdUser.Role.ToString(),
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                    };

                    _logger.LogInformation("User registered successfully: {UserId}", createdUser.Id);
                    return Result<AuthResponseDto>.Success(response);
                }
                catch (Exception)
                {
                    // Hata durumunda rollback
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure("Registration failed");
            }
        }
    }
}
