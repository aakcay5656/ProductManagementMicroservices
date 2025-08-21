using System.Linq.Expressions;

namespace Shared.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Temel crud iþlemleri
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);


        // geliþmiþ sorgular
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
    }
}
