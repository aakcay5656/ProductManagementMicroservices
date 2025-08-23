using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Features.Queries;
using Product.Core.Interfaces;
using Shared.Common.Models;

namespace Product.Application.Handlers
{
    public class GetUserProductsQueryHandler : IRequestHandler<GetUserProductsQuery, Result<PaginatedProductResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetUserProductsQueryHandler> _logger;

        public GetUserProductsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<GetUserProductsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<PaginatedProductResponseDto>> Handle(GetUserProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting products for user: {UserId}, Page: {Page}", request.UserId, request.Page);

                // Cache key oluştur
                var cacheKey = $"user_products:{request.UserId}:page_{request.Page}:size_{request.PageSize}";
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    cacheKey += $":search_{request.SearchTerm}";
                }

                // Cache'den kontrol et
                var cachedResult = await _cacheService.GetAsync<PaginatedProductResponseDto>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("User products retrieved from cache: {UserId}", request.UserId);
                    return Result<PaginatedProductResponseDto>.Success(cachedResult);
                }

                // Calculate skip
                var skip = (request.Page - 1) * request.PageSize;

                // Database'den kullanıcının ürünlerini getir
                var products = await _unitOfWork.Products.GetProductsAsync(
                    searchTerm: request.SearchTerm,
                    userId: request.UserId,
                    skip: skip,
                    take: request.PageSize);

                var totalCount = await _unitOfWork.Products.GetProductsCountAsync(
                    searchTerm: request.SearchTerm,
                    userId: request.UserId);

                // DTOs'a map et
                var productDtos = _mapper.Map<IEnumerable<ProductResponseDto>>(products);

                // Paginated response oluştur
                var response = new PaginatedProductResponseDto
                {
                    Products = productDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                    HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
                    HasPreviousPage = request.Page > 1
                };

                // Cache'e kaydet (3 dakika)
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(3));

                _logger.LogInformation("User products retrieved from database: {UserId}, Count: {Count}",
                    request.UserId, productDtos.Count());

                return Result<PaginatedProductResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for user: {UserId}", request.UserId);
                return Result<PaginatedProductResponseDto>.Failure("Failed to retrieve user products");
            }
        }
    }
}
