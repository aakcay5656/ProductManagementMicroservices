using Auth.Core.Entities;
using Shared.Common.Interfaces;

namespace Auth.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        // User'a özel metodlar
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    }
}
