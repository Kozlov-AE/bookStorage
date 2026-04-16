using BookStorage.Infrastructure.Configuration;

namespace BookStorage.Infrastructure.Services;

public class LocalFileStorageService : BookStorage.Core.Interfaces.Infrastructure.IFileStorageService
{
    private readonly string _booksPath;

    public LocalFileStorageService(StorageOptions options)
    {
        _booksPath = options.BooksPath;
        Directory.CreateDirectory(_booksPath);
    }

    public async Task<string> SaveBookAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = GetSafeFileName(fileName);
        var filePath = Path.Combine(_booksPath, safeFileName);
        
        await using var outputStream = File.Create(filePath);
        await fileStream.CopyToAsync(outputStream, cancellationToken);
        
        return safeFileName;
    }

    public Task<Stream?> GetBookAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = GetSafeFileName(fileName);
        var filePath = Path.Combine(_booksPath, safeFileName);
        
        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);
        
        Stream stream = File.OpenRead(filePath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteBookAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = GetSafeFileName(fileName);
        var filePath = Path.Combine(_booksPath, safeFileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetBookListAsync(CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(_booksPath).Select(Path.GetFileName);
        return Task.FromResult(files.Where(f => f != null).Cast<string>().AsEnumerable());
    }

    private static string GetSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return safeName.Replace(" ", "_");
    }
}
