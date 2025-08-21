using Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Auth.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        // Constructor - DI ile DbContextOptions al�r
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        // DbSet'ler - Her entity i�in bir tablo
        public DbSet<User> Users { get; set; }

        // Model configuration - Entity'lerin database mapping'i
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity konfig�rasyonu
            modelBuilder.Entity<User>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Email unique olmal� ve index olmal�
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();

                // Password hash
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                // �sim alanlar�
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

                // Timestamps - default de�erler
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Performans i�in index'ler
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.RefreshToken);
            });
        }

        // SaveChanges override - UpdatedAt otomatik g�ncelle
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Shared.Common.Entities.BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (Shared.Common.Entities.BaseEntity)entityEntry.Entity;

                // Yeni kay�t ise CreatedAt set et
                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                // Her durumda UpdatedAt g�ncelle
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
