using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Features.Queries;
using Product.Core.Interfaces;
using Shared.Common.Models;

namespace Product.Application.Handlers
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PaginatedProductResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetProductsQueryHandler> _logger;

        public GetProductsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<GetProductsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Result<PaginatedProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting products with filters: Search={Search}, Category={Category}, Page={Page}",
                    request.SearchTerm, request.Category, request.Page);

                // Cache key oluştur
                var cacheKey = GenerateCacheKey(request);

                // Cache'den kontrol et
                var cachedResult = await _cacheService.GetAsync<PaginatedProductResponseDto>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Products retrieved from cache: {CacheKey}", cacheKey);
                    return Result<PaginatedProductResponseDto>.Success(cachedResult);
                }

                // Calculate skip
                var skip = (request.Page - 1) * request.PageSize;

                // Database'den getir
                var products = await _unitOfWork.Products.GetProductsAsync(
                    searchTerm: request.SearchTerm,
                    category: request.Category,
                    minPrice: request.MinPrice,
                    maxPrice: request.MaxPrice,
                    skip: skip,
                    take: request.PageSize);

                var totalCount = await _unitOfWork.Products.GetProductsCountAsync(
                    searchTerm: request.SearchTerm,
                    category: request.Category,
                    minPrice: request.MinPrice,
                    maxPrice: request.MaxPrice);

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

                // Cache'e kaydet (5 dakika)
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Products retrieved from database: {Count} products", productDtos.Count());

                return Result<PaginatedProductResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return Result<PaginatedProductResponseDto>.Failure("Failed to retrieve products");
            }
        }

        private static string GenerateCacheKey(GetProductsQuery request)
        {
            var keyParts = new List<string> { "products" };

            if (!string.IsNullOrEmpty(request.SearchTerm))
                keyParts.Add($"search_{request.SearchTerm}");

            if (request.Category.HasValue)
                keyParts.Add($"cat_{request.Category}");

            if (request.MinPrice.HasValue)
                keyParts.Add($"minp_{request.MinPrice}");

            if (request.MaxPrice.HasValue)
                keyParts.Add($"maxp_{request.MaxPrice}");

            keyParts.Add($"page_{request.Page}");
            keyParts.Add($"size_{request.PageSize}");
            keyParts.Add($"sort_{request.SortBy}_{request.SortDescending}");

            return string.Join(":", keyParts);
        }
    }
}
