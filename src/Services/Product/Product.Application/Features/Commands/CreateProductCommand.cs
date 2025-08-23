using MediatR;
using Product.Application.DTOs;
using Product.Core.Enums;
using Shared.Common.Models;

namespace Product.Application.Features.Commands
{
    public class CreateProductCommand : IRequest<Result<ProductResponseDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public ProductCategory Category { get; set; }
        public string? ImageUrl { get; set; }
        public string? MetaDescription { get; set; }

        // JWT'den gelecek
        public int UserId { get; set; }
    }
}
