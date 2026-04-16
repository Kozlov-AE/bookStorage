namespace BookStorage.Core.Interfaces.Infrastructure;

public interface IFileStorageService
{
    Task<string> SaveBookAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream?> GetBookAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteBookAsync(string fileName, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetBookListAsync(CancellationToken cancellationToken = default);
}
