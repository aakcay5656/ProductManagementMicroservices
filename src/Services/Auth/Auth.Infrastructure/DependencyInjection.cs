using Auth.Core.Interfaces;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using Auth.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // PostgreSQL Database
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("AuthConnection")));

            // Repository Pattern
            services.AddScoped<IUserRepository, UserRepository>();

            // Unit of Work Pattern
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // Services
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
