using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Features.Queries;
using Product.Core.Interfaces;
using Shared.Common.Models;

namespace Product.Application.Handlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;

        public GetProductByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<GetProductByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting product with ID: {ProductId}", request.Id);

                // Cache'den kontrol et
                var cacheKey = $"product:{request.Id}";
                var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(cacheKey);

                if (cachedProduct != null)
                {
                    return Result<ProductResponseDto>.Success(cachedProduct);
                }

                // Database'den getir
                var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

                if (product == null)
                {
                    return Result<ProductResponseDto>.Failure("Product not found");
                }

                // DTO'ya map et
                var productDto = _mapper.Map<ProductResponseDto>(product);

                // Cache'e kaydet
                await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(5));

                return Result<ProductResponseDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID: {ProductId}", request.Id);
                return Result<ProductResponseDto>.Failure("Failed to retrieve product");
            }
        }
    }
}
