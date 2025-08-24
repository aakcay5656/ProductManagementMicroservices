namespace Shared.Services
{
    public interface ILogService
    {
        Task LogInformationAsync(string service, string message, object? data = null);
        Task LogWarningAsync(string service, string message, object? data = null);
        Task LogErrorAsync(string service, string message, object? data = null, string? exception = null);
        Task LogCriticalAsync(string service, string message, object? data = null, string? exception = null);
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
