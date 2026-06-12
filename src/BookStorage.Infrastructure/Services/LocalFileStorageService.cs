using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStorage.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly StorageOptions _options;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptionsSnapshot<StorageOptions> options, IUnitOfWork uow, ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _uow = uow;
        _logger = logger;
    }

    public async Task<BookFile> SaveBookAsync(Stream fileStream, string fileType, Book book,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving book file: {BookTitle}, FileType: {FileType}", book.Title, fileType);
        var categoryPath = await GetCategoryPathAsync(book.CategoryId, cancellationToken);
        var safeFileName = GetSafeFileName(book.Title);
        var relativePath = BuildRelativePath(categoryPath, safeFileName + "." + fileType);
        var fullPath = Path.Combine(_options.BooksPath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogDebug("Created directory for book storage: {DirectoryPath}", directory);
        }

        await using var outputStream = File.Create(fullPath);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        var fileInfo = new FileInfo(fullPath);
        _logger.LogInformation("Book file saved successfully: {FileName}, Size: {FileSizeBytes} bytes", safeFileName, fileInfo.Length);

        return new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = safeFileName,
            FileType = fileType,
            FullFilePath = relativePath,
            FileSizeBytes = fileInfo.Length,
            UploadedAt = DateTime.UtcNow,
        };
    }

    private async Task<string> GetCategoryPathAsync(Guid? categoryId, CancellationToken ct)
    {
        if (!categoryId.HasValue || categoryId.Value == Guid.Empty)
            return string.Empty;

        var pathParts = new List<string>();
        var categories =  (await _uow.Categories.GetAllAsync(ct)).ToArray();
        var category = categories.FirstOrDefault(c => c.Id == categoryId.Value);
        while (category != null)
        {
            pathParts.Add(category.Name);
            if (!category.ParentCategoryId.HasValue || category.ParentCategoryId.Value == Guid.Empty)
                break;
            category = categories.FirstOrDefault(c => c.Id == category.ParentCategoryId.Value);
        }

        pathParts.Reverse();
        return Path.Combine(pathParts.ToArray());

    }

    public Task<Stream?> GetBookAsync(string fileNameOrRelativePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting book file: {FileNameOrRelativePath}", fileNameOrRelativePath);
        var filePath = GetFullPath(fileNameOrRelativePath);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Book file not found: {FilePath}", filePath);
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(filePath);
        _logger.LogInformation("Book file opened successfully: {FilePath}", filePath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteBookAsync(string fileNameOrRelativePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting book file: {FileNameOrRelativePath}", fileNameOrRelativePath);
        var filePath = GetFullPath(fileNameOrRelativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Book file deleted successfully: {FilePath}", filePath);
            return Task.FromResult(true);
        }

        _logger.LogWarning("Failed to delete book file (not found): {FilePath}", filePath);
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetBookListAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting list of all books from storage");
        var files = Directory.GetFiles(_options.BooksPath, "*", SearchOption.AllDirectories)
            .Select(GetRelativePath)
            .Where(f => !string.IsNullOrEmpty(f));
        var fileList = files.ToList();
        _logger.LogInformation("Retrieved {BooksCount} book files from storage", fileList.Count);

        return Task.FromResult((IEnumerable<string>)fileList);
    }

    private string BuildRelativePath(string? categoryPath, string fileName)
    {
        if (string.IsNullOrEmpty(categoryPath))
            return fileName;

        var safeCategoryPath = GetSafePath(categoryPath);
        return Path.Combine(safeCategoryPath, fileName);
    }

    private string GetRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(_options.BooksPath, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath[_options.BooksPath.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        return fullPath;
    }

    private string GetFullPath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            return relativePath;
        return Path.Combine(_options.BooksPath, relativePath);
    }

    private static string GetSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return safeName.Replace(" ", "_");
    }

    private static string GetSafePath(string path)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safePath = string.Join(Path.DirectorySeparatorChar.ToString(),
            path.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return safePath.Replace(" ", "_");
    }
}