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

                // 1. Email zaten var m�?
                if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
                {
                    return Result<AuthResponseDto>.Failure("Email already exists");
                }

                // 2. Transaction ba�lat (ACID properties i�in)
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 3. User olu�tur
                    var user = new User
                    {
                        Email = request.Email,
                        PasswordHash = _authService.HashPassword(request.Password),
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Role = UserRole.User,
                        IsActive = true
                    };

                    // 4. User'� kaydet
                    var createdUser = await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    // 5. JWT ve Refresh token olu�tur
                    var accessToken = await _authService.GenerateJwtTokenAsync(
                        createdUser.Id,
                        createdUser.Email,
                        createdUser.Role
                    );
                    var refreshToken = await _authService.GenerateRefreshTokenAsync();

                    // 6. Refresh token'� user'a kaydet
                    createdUser.RefreshToken = refreshToken;
                    createdUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
                    await _unitOfWork.Users.UpdateAsync(createdUser);
                    await _unitOfWork.SaveChangesAsync();

                    // 7. Transaction'� commit et
                    await _unitOfWork.CommitTransactionAsync();

                    // 8. Response olu�tur
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
