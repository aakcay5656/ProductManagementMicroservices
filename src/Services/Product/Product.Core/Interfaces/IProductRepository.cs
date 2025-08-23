using Product.Core.Entities;
using Product.Core.Enums;
using Shared.Common.Interfaces;

namespace Product.Core.Interfaces
{
    public interface IProductRepository : IRepository<Product.Core.Entities.Product>
    {
        // User-specific queries
        Task<IEnumerable<Product.Core.Entities.Product>> GetProductsByUserIdAsync(int userId);
        Task<IEnumerable<Product.Core.Entities.Product>> GetActiveProductsByUserIdAsync(int userId);

        // Public queries (for product listing)
        Task<IEnumerable<Product.Core.Entities.Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product.Core.Entities.Product>> GetProductsByCategoryAsync(ProductCategory category);
        Task<IEnumerable<Product.Core.Entities.Product>> SearchProductsAsync(string searchTerm);

        // Advanced queries
        Task<IEnumerable<Product.Core.Entities.Product>> GetProductsAsync(
            string? searchTerm = null,
            ProductCategory? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? userId = null,
            int skip = 0,
            int take = 10);

        Task<int> GetProductsCountAsync(
            string? searchTerm = null,
            ProductCategory? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? userId = null);

        // Business logic queries
        Task<bool> IsProductOwnerAsync(int productId, int userId);
        Task<IEnumerable<Product.Core.Entities.Product>> GetLowStockProductsAsync(int threshold = 5);
        Task<Product.Core.Entities.Product?> GetBySlugAsync(string slug);

        // Stock management
        Task<bool> UpdateStockAsync(int productId, int newStock);
        Task<bool> DecrementStockAsync(int productId, int quantity);
    }
}
