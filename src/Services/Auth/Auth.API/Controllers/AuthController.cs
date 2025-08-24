using Auth.Application.DTOs;
using Auth.Application.Features.Commands;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Events;
using MassTransit;
using Shared.Services;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            IPublishEndpoint publishEndpoint,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var command = new RegisterCommand
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName
                };

                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                // Event publish
                var userCreatedEvent = new UserCreatedEvent
                {
                    UserId = result.Data!.Id,
                    Email = result.Data.Email,
                    FirstName = result.Data.FirstName,
                    LastName = result.Data.LastName,
                    CreatedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(userCreatedEvent);

                _logger.LogInformation("📤 UserCreatedEvent published for UserId: {UserId}", result.Data.Id);

                return CreatedAtAction(nameof(Register), new
                {
                    message = "User registered successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var query = new LoginCommand { Email = loginDto.Email, Password = loginDto.Password };
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return Unauthorized(new { message = result.ErrorMessage });
                }

                return Ok(new { message = "Login successful", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var command = new RefreshTokenCommand
                {
                    RefreshToken = refreshTokenDto.RefreshToken,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };

                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return Unauthorized(new { message = result.ErrorMessage });
                }

                return Ok(new
                {
                    message = "Token refreshed successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private string GetClientIpAddress()
        {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "Unknown";
        }

    
        
    }
}
