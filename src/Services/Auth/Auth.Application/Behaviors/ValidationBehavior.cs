using FluentValidation;
using MediatR;
using Shared.Common.Models;

namespace Auth.Application.Behaviors
{
    // MediatR pipeline'ında validation behavior
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class, IRequest<TResponse>
        where TResponse : Result, new()
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Eğer validator'lar varsa çalıştır
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                // Tüm validator'ları paralel olarak çalıştır
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                // Hataları topla
                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                // Eğer validation hatası varsa Result.Failure döndür
                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();
                    var response = new TResponse();

                    // Reflection ile Result propertylerini set et
                    typeof(TResponse).GetProperty("IsSuccess")?.SetValue(response, false);
                    typeof(TResponse).GetProperty("ErrorMessage")?.SetValue(response, "Validation failed");
                    typeof(TResponse).GetProperty("Errors")?.SetValue(response, errors);

                    return response;
                }
            }

            // Validation başarılıysa normal işleme devam et
            return await next();
        }
    }
}
