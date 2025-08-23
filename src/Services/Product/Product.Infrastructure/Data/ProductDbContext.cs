using Microsoft.EntityFrameworkCore;
using Product.Core.Entities;
using Product.Core.Enums;

namespace Product.Infrastructure.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        public DbSet<Product.Core.Entities.Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product entity configuration
            modelBuilder.Entity<Product.Core.Entities.Product>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Name - required, indexed for search
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.HasIndex(e => e.Name);

                // Description
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                // Price - decimal precision
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasPrecision(18, 2);

                // Stock
                entity.Property(e => e.Stock)
                    .IsRequired()
                    .HasDefaultValue(0);

                // Category enum to int
                entity.Property(e => e.Category)
                    .HasConversion<int>()
                    .IsRequired();

                // Status enum to int
                entity.Property(e => e.Status)
                    .HasConversion<int>()
                    .IsRequired()
                    .HasDefaultValue(ProductStatus.Active);

                // UserId - foreign key relationship
                entity.Property(e => e.UserId)
                    .IsRequired();

                // Optional fields
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500);

                // Slug - unique, indexed for SEO
                entity.Property(e => e.Slug)
                    .HasMaxLength(200);
                entity.HasIndex(e => e.Slug);

                entity.Property(e => e.MetaDescription)
                    .HasMaxLength(160);

                // Timestamps
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Composite indexes for performance
                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.HasIndex(e => new { e.Category, e.Status });
                entity.HasIndex(e => new { e.Price, e.Status });
                entity.HasIndex(e => new { e.CreatedAt, e.Status });

                // Full-text search index (PostgreSQL specific)
              //  entity.HasIndex(e => new { e.Name, e.Description })
                //    .HasMethod("gin")
                 //   .HasOperators("gin_trgm_ops");
            });

            // Seed data (optional)
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product.Core.Entities.Product>().HasData(
                new Product.Core.Entities.Product
                {
                    Id = 1,
                    Name = "Sample iPhone 15",
                    Description = "Latest iPhone with advanced features",
                    Price = 999.99m,
                    Stock = 50,
                    Category = ProductCategory.Electronics,
                    Status = ProductStatus.Active,
                    UserId = 1,
                    Slug = "sample-iphone-15",
                    //  DÜZELTME: Static datetime kullan
                    CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                },
                new Product.Core.Entities.Product
                {
                    Id = 2,
                    Name = "Programming Book",
                    Description = "Learn .NET microservices architecture",
                    Price = 49.99m,
                    Stock = 100,
                    Category = ProductCategory.Books,
                    Status = ProductStatus.Active,
                    UserId = 1,
                    Slug = "programming-book",
                    //  DÜZELTME: Static datetime kullan
                    CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        // Auto-update timestamps
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Shared.Common.Entities.BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (Shared.Common.Entities.BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
