using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Log.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        [HttpPost("information")]
        public IActionResult LogInformation([FromBody] LogRequest request)
        {
            _logger.LogInformation("{Service} - {Message} | {@Data}",
                request.Service, request.Message, request.Data);

            return Ok(new { Message = "Information logged successfully", Timestamp = DateTime.UtcNow });
        }

        [HttpPost("warning")]
        public IActionResult LogWarning([FromBody] LogRequest request)
        {
            _logger.LogWarning("{Service} - {Message} | {@Data}",
                request.Service, request.Message, request.Data);

            return Ok(new { Message = "Warning logged successfully", Timestamp = DateTime.UtcNow });
        }

        [HttpPost("error")]
        public IActionResult LogError([FromBody] LogRequest request)
        {
            _logger.LogError("{Service} - {Message} | {@Data} | {Exception}",
                request.Service, request.Message, request.Data, request.Exception);

            return Ok(new { Message = "Error logged successfully", Timestamp = DateTime.UtcNow });
        }

        [HttpPost("critical")]
        public IActionResult LogCritical([FromBody] LogRequest request)
        {
            _logger.LogCritical("{Service} - {Message} | {@Data} | {Exception}",
                request.Service, request.Message, request.Data, request.Exception);

            return Ok(new { Message = "Critical log recorded successfully", Timestamp = DateTime.UtcNow });
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Log Service test endpoint called at {Timestamp}", DateTime.UtcNow);
            _logger.LogWarning("This is a warning test");
            _logger.LogError("This is an error test");

            return Ok(new
            {
                Message = "Log Service is working correctly",
                Timestamp = DateTime.UtcNow,
                Levels = new[] { "Information", "Warning", "Error", "Critical" }
            });
        }
    }

    public class LogRequest
    {
        public string Service { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string? Exception { get; set; }
        public string Level { get; set; } = "Information";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
