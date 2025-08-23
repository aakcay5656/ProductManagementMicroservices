using MediatR;
using Product.Application.DTOs;
using Shared.Common.Models;

namespace Product.Application.Features.Queries
{
    public class GetUserProductsQuery : IRequest<Result<PaginatedProductResponseDto>>
    {
        public int UserId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
