using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Core.Interfaces;
using Product.Infrastructure.Cache;
using Product.Infrastructure.Data;
using Product.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Product.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // PostgreSQL Database - Migration assembly belirt
            services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("ProductConnection"),
                    b => b.MigrationsAssembly("Product.Infrastructure") // ← Bu satırı ekle
                ));

            // Redis Cache
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(connectionString);
            });

            // Repository Pattern
            services.AddScoped<IProductRepository, ProductRepository>();

            // Unit of Work Pattern
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Cache Service
            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }
    }
}
