using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Features.Commands;
using Product.Core.Interfaces;
using Shared.Common.Models;

namespace Product.Application.Handlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<CreateProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<ProductResponseDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating product: {ProductName} for user: {UserId}", request.Name, request.UserId);

                // Transaction baþlat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Command'i Entity'ye map et
                    var product = _mapper.Map<Product.Core.Entities.Product>(request);

                    // Product'ý kaydet
                    var createdProduct = await _unitOfWork.Products.AddAsync(product);
                    await _unitOfWork.SaveChangesAsync();

                    // Transaction commit
                    await _unitOfWork.CommitTransactionAsync();

                    // Cache invalidation - product list cache'ini temizle
                    await InvalidateProductCaches();

                    // Response DTO oluþtur
                    var response = _mapper.Map<ProductResponseDto>(createdProduct);

                    _logger.LogInformation("Product created successfully: {ProductId}", createdProduct.Id);

                    // TODO: Event publishing (ProductCreatedEvent)
                    // await _eventBus.PublishAsync(new ProductCreatedEvent { ProductId = createdProduct.Id, UserId = request.UserId });

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
                _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
                return Result<ProductResponseDto>.Failure("Failed to create product");
            }
        }

        private async Task InvalidateProductCaches()
        {
            try
            {
                // Product list cache'lerini temizle
                await _cacheService.RemoveByPatternAsync("products:*");
                await _cacheService.RemoveByPatternAsync("user_products:*");

                _logger.LogInformation("Product caches invalidated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate product caches");
                // Cache invalidation failure'ý critical deðil, operation devam etsin
            }
        }
    }
}
