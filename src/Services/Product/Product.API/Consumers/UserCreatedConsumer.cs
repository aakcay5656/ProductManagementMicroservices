using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Services;

namespace Product.API.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly ILogService _logService; // Ekle

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("🎉 UserCreatedEvent received - UserId: {UserId}", message.UserId);

            // Centralized logging
            await _logService.LogInformationAsync("ProductService",
                "UserCreatedEvent consumed successfully",
                new
                {
                    UserId = message.UserId,
                    Email = message.Email,
                    EventType = "UserCreated",
                    ProcessedAt = DateTime.UtcNow
                });

            await Task.CompletedTask;
        }
    }

}
