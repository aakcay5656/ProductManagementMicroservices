using FluentValidation;
using Product.Application.Features.Commands;
using Product.Core.Enums;

namespace Product.Application.Validators
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters")
                .MinimumLength(2).WithMessage("Product name must be at least 2 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Price cannot exceed 1,000,000");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
                .LessThanOrEqualTo(100000).WithMessage("Stock cannot exceed 100,000");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid product category");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).WithMessage("Invalid image URL")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.MetaDescription)
                .MaximumLength(160).WithMessage("Meta description must not exceed 160 characters");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");
        }

        private static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
