using Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Auth.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        // Constructor - DI ile DbContextOptions alýr
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        // DbSet'ler - Her entity için bir tablo
        public DbSet<User> Users { get; set; }

        // Model configuration - Entity'lerin database mapping'i
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity konfigürasyonu
            modelBuilder.Entity<User>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Email unique olmalý ve index olmalý
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();

                // Password hash
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                // Ýsim alanlarý
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                // Enum to int conversion
                entity.Property(e => e.Role)
                    .HasConversion<int>();

                // Optional fields
                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(500);

                // Timestamps - default deðerler
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Performans için index'ler
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.RefreshToken);
            });
        }

        // SaveChanges override - UpdatedAt otomatik güncelle
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Shared.Common.Entities.BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (Shared.Common.Entities.BaseEntity)entityEntry.Entity;

                // Yeni kayýt ise CreatedAt set et
                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                // Her durumda UpdatedAt güncelle
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
