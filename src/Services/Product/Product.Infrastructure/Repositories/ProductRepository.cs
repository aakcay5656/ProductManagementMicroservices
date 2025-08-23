using Microsoft.EntityFrameworkCore;
using Product.Core.Entities;
using Product.Core.Enums;
using Product.Core.Interfaces;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product.Core.Entities.Product>, IProductRepository
    {
        public ProductRepository(ProductDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> GetProductsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> GetActiveProductsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.Status == ProductStatus.Active)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == ProductStatus.Active)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> GetProductsByCategoryAsync(ProductCategory category)
        {
            return await _dbSet
                .Where(p => p.Category == category && p.Status == ProductStatus.Active)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> SearchProductsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.Status == ProductStatus.Active &&
                           (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> GetProductsAsync(
            string? searchTerm = null,
            ProductCategory? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? userId = null,
            int skip = 0,
            int take = 10)
        {
            var query = _dbSet.Where(p => p.Status == ProductStatus.Active);

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            if (category.HasValue)
            {
                query = query.Where(p => p.Category == category.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetProductsCountAsync(
            string? searchTerm = null,
            ProductCategory? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? userId = null)
        {
            var query = _dbSet.Where(p => p.Status == ProductStatus.Active);

            // Apply same filters as GetProductsAsync
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            if (category.HasValue)
            {
                query = query.Where(p => p.Category == category.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> IsProductOwnerAsync(int productId, int userId)
        {
            return await _dbSet
                .AnyAsync(p => p.Id == productId && p.UserId == userId);
        }

        public async Task<IEnumerable<Product.Core.Entities.Product>> GetLowStockProductsAsync(int threshold = 5)
        {
            return await _dbSet
                .Where(p => p.Status == ProductStatus.Active && p.Stock <= threshold)
                .OrderBy(p => p.Stock)
                .ToListAsync();
        }

        public async Task<Product.Core.Entities.Product?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ProductStatus.Active);
        }

        public async Task<bool> UpdateStockAsync(int productId, int newStock)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product == null) return false;

            product.Stock = newStock;
            return true;
        }

        public async Task<bool> DecrementStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product == null || product.Stock < quantity) return false;

            product.Stock -= quantity;
            return true;
        }
    }
}
