using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IBookFileRepository
{
    Task<IEnumerable<BookFile>> GetByBookIdAsync(int bookId, CancellationToken ct = default);
    Task<BookFile?> GetByHashAsync(string hash, CancellationToken ct = default);
    Task<BookFile?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BookFile> AddAsync(BookFile file, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
