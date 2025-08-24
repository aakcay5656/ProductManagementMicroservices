using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    public class HttpLogService : ILogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpLogService> _logger;
        private readonly string _logServiceUrl;

        public HttpLogService(HttpClient httpClient, ILogger<HttpLogService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _logServiceUrl = "https://localhost:5003/api/v1/log";
        }

        public async Task LogInformationAsync(string service, string message, object? data = null)
        {
            await SendLogAsync("information", service, message, data);
        }

        public async Task LogWarningAsync(string service, string message, object? data = null)
        {
            await SendLogAsync("warning", service, message, data);
        }

        public async Task LogErrorAsync(string service, string message, object? data = null, string? exception = null)
        {
            await SendLogAsync("error", service, message, data, exception);
        }

        public async Task LogCriticalAsync(string service, string message, object? data = null, string? exception = null)
        {
            await SendLogAsync("critical", service, message, data, exception);
        }

        private async Task SendLogAsync(string level, string service, string message, object? data = null, string? exception = null)
        {
            try
            {
                var logRequest = new LogRequest
                {
                    Service = service,
                    Message = message,
                    Data = data,
                    Exception = exception,
                    Level = level,
                    Timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(logRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_logServiceUrl}/{level}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send log to Log Service. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending log to Log Service");
            }
        }
    }
}
