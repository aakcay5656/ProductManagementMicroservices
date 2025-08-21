namespace Auth.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository'ler
        IUserRepository Users { get; }

        // Transaction operations
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
