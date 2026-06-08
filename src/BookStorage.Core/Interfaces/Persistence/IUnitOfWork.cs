using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IBookRepository Books { get; }
    IBookFileRepository BookFiles { get; }
    IPersonRepository Persons { get; }
    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}