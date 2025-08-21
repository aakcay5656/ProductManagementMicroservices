using Auth.Application.DTOs;
using MediatR;
using Shared.Common.Models;

namespace Auth.Application.Features.Commands
{
    public class RefreshTokenCommand:IRequest<Result<AuthResponseDto>>
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? IpAddress {  get; set; }
        public string? UserAgent {  get; set; }
    }
}
