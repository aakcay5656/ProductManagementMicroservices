using Auth.Application.DTOs;
using MediatR;
using Shared.Common.Models;

namespace Auth.Application.Features.Commands
{
    // Register komutu - MediatR IRequest interface'ini implement ediyor
    public class RegisterCommand : IRequest<Result<AuthResponseDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Ek bilgiler (opsiyonel)
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
