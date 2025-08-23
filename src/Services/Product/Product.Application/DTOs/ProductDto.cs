using Product.Core.Enums;

namespace Product.Application.DTOs
{
    // Product response DTO
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Slug { get; set; }
        public bool IsActive { get; set; }
        public bool IsInStock { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Create product DTO
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public ProductCategory Category { get; set; }
        public string? ImageUrl { get; set; }
        public string? MetaDescription { get; set; }
    }

    // Update product DTO
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public ProductCategory Category { get; set; }
        public ProductStatus Status { get; set; }
        public string? ImageUrl { get; set; }
        public string? MetaDescription { get; set; }
    }

    // Product query filter DTO
    public class ProductQueryDto
    {
        public string? SearchTerm { get; set; }
        public ProductCategory? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    // Paginated response DTO
    public class PaginatedProductResponseDto
    {
        public IEnumerable<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
