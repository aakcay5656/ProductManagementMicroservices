using FluentValidation;
using MediatR;
using Shared.Common.Models;

namespace Product.Application.Behaviors
{
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
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();
                    var response = new TResponse();

                    typeof(TResponse).GetProperty("IsSuccess")?.SetValue(response, false);
                    typeof(TResponse).GetProperty("ErrorMessage")?.SetValue(response, "Validation failed");
                    typeof(TResponse).GetProperty("Errors")?.SetValue(response, errors);

                    return response;
                }
            }

            return await next();
        }
    }
}
