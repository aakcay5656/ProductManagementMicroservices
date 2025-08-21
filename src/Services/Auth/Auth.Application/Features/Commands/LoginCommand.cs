using Auth.Application.DTOs;
using MediatR;
using Shared.Common.Models;


namespace Auth.Application.Features.Commands
{
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? IpAddress {  get; set; }
        public string? UserAgent { get; set; }
    }
}
