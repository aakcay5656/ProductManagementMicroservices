using MediatR;
using Microsoft.Extensions.Logging;
using Product.Application.Features.Commands;
using Product.Core.Interfaces;
using Shared.Common.Models;

namespace Product.Application.Handlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<DeleteProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting product: {ProductId} for user: {UserId}", request.Id, request.UserId);

                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Ürünü bul
                    var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
                    if (product == null)
                    {
                        return Result.Failure("Product not found");
                    }

                    // Authorization check - Sadece ürün sahibi silebilir
                    if (product.UserId != request.UserId)
                    {
                        return Result.Failure("You are not authorized to delete this product");
                    }

                    // Database'den sil
                    var success = await _unitOfWork.Products.DeleteAsync(request.Id);
                    if (!success)
                    {
                        return Result.Failure("Failed to delete product");
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Transaction commit
                    await _unitOfWork.CommitTransactionAsync();

                    // Cache invalidation
                    await InvalidateProductCaches(request.Id, request.UserId);

                    _logger.LogInformation("Product deleted successfully: {ProductId}", request.Id);

                    return Result.Success();
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", request.Id);
                return Result.Failure("Failed to delete product");
            }
        }

        private async Task InvalidateProductCaches(int productId, int userId)
        {
            try
            {
                await _cacheService.RemoveAsync($"product:{productId}");
                await _cacheService.RemoveByPatternAsync("products:*");
                await _cacheService.RemoveByPatternAsync($"user_products:{userId}:*");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate caches for product: {ProductId}", productId);
            }
        }
    }
}
