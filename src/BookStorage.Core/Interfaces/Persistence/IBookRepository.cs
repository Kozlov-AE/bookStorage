using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Book> AddAsync(Book book, CancellationToken ct = default);
    Task<Book?> UpdateAsync(Guid id, Book book, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
