using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Features.Commands;
using Product.Core.Interfaces;
using Shared.Common.Models;

namespace Product.Application.Handlers
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<ProductResponseDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating product: {ProductId} for user: {UserId}", request.Id, request.UserId);

                // Transaction başlat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Ürünü bul
                    var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
                    if (product == null)
                    {
                        return Result<ProductResponseDto>.Failure("Product not found");
                    }

                    // Authorization check - Sadece ürün sahibi güncelleyebilir
                    if (product.UserId != request.UserId)
                    {
                        return Result<ProductResponseDto>.Failure("You are not authorized to update this product");
                    }

                    // Update mapping
                    _mapper.Map(request, product);
                    product.UpdatedAt = DateTime.UtcNow;

                    // Database güncelle
                    var updatedProduct = await _unitOfWork.Products.UpdateAsync(product);
                    await _unitOfWork.SaveChangesAsync();

                    // Transaction commit
                    await _unitOfWork.CommitTransactionAsync();

                    // Cache invalidation
                    await InvalidateProductCaches(request.Id, request.UserId);

                    // Response DTO
                    var response = _mapper.Map<ProductResponseDto>(updatedProduct);

                    _logger.LogInformation("Product updated successfully: {ProductId}", request.Id);

                    return Result<ProductResponseDto>.Success(response);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", request.Id);
                return Result<ProductResponseDto>.Failure("Failed to update product");
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
