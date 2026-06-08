using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Application;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Book?> CreateAsync(Book book, Stream fileStream, string fileType, CancellationToken ct = default);
    Task<Book?> UpdateAsync(Guid id, Book book, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}