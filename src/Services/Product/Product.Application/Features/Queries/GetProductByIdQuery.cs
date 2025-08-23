using MediatR;
using Product.Application.DTOs;
using Shared.Common.Models;

namespace Product.Application.Features.Queries
{
    public class GetProductByIdQuery : IRequest<Result<ProductResponseDto>>
    {
        public int Id { get; set; }
    }
}
