using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Infrastructure;

public interface IFileStorageService
{
    Task<BookFile> SaveBookAsync(Stream fileStream, string fileType, Book book,
        CancellationToken cancellationToken = default);
    Task<Stream?> GetBookAsync(string fileNameOrRelativePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteBookAsync(string fileNameOrRelativePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetBookListAsync(CancellationToken cancellationToken = default);
}
