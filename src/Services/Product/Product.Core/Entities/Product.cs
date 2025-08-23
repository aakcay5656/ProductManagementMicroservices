using Product.Core.Enums;
using Shared.Common.Entities;
using System.ComponentModel.DataAnnotations;

namespace Product.Core.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public ProductCategory Category { get; set; } = ProductCategory.Other;

        public ProductStatus Status { get; set; } = ProductStatus.Active;

        // User relationship - Bu ürün hangi user'a ait
        [Required]
        public int UserId { get; set; }

        // Product images (optional)
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // SEO fields
        [MaxLength(200)]
        public string? Slug { get; set; }

        [MaxLength(160)]
        public string? MetaDescription { get; set; }

        // Business logic properties
        public bool IsActive => Status == ProductStatus.Active;
        public bool IsInStock => Stock > 0;
        public bool IsAvailable => IsActive && IsInStock;

        // Navigation properties (for future extensions)
        // public User User { get; set; } = null!; // Foreign key navigation
        // public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}
