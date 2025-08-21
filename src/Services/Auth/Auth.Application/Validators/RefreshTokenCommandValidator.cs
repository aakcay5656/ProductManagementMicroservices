using Auth.Application.Features.Commands;
using FluentValidation;

namespace Auth.Application.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required")
                .MinimumLength(10).WithMessage("Invalid refresh token format");
        }
    }
}
