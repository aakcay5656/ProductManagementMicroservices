using Microsoft.EntityFrameworkCore.Storage;
using Product.Core.Interfaces;
using Product.Infrastructure.Repositories;

namespace Product.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _context;
        private IProductRepository? _products;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ProductDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products => _products ??= new ProductRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.CommitAsync();
                }
                catch
                {
                    await _transaction.RollbackAsync();
                    throw;
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
