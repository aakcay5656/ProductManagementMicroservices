using MediatR;
using Product.Application.DTOs;
using Product.Core.Enums;
using Shared.Common.Models;

namespace Product.Application.Features.Queries
{
    public class GetProductsQuery : IRequest<Result<PaginatedProductResponseDto>>
    {
        public string? SearchTerm { get; set; }
        public ProductCategory? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}
