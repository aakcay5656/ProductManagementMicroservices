using MediatR;
using Shared.Common.Models;

namespace Product.Application.Features.Commands
{
    public class DeleteProductCommand : IRequest<Result>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Authorization için
    }
}
