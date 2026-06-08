using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IBookRepository: IRepository<Book>
{
    Task<Book?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
