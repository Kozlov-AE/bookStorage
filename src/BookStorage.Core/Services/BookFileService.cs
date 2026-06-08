using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;
using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace BookStorage.Core.Services;

public class BookFileService : IBookFileService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fs;
    private readonly ILogger<BookFileService> _logger;

    public BookFileService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, ILogger<BookFileService> logger)
    {
        _uow = unitOfWork;
        _fs = fileStorageService;
        _logger = logger;
    }

    public async Task<(Stream? Content, BookFile? Metadata)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var file = await _uow.BookFiles.GetByIdAsync(id, ct);
        if (file == null)
        {
            _logger.LogWarning("Book file with ID {FileId} not found in DB", id);
            return (null, null);
        }

        _logger.LogDebug("Getting file from storage: {FileName}", file.FileName);
        var stream = await _fs.GetBookAsync(file.FullFilePath ?? file.FileName, ct);
        
        if (stream == null)
        {
            _logger.LogWarning("File not found in storage: {FileName}", file.FileName);
            return (null, null);
        }

        _logger.LogInformation("File retrieved successfully: {FileName}", file.FileName);
        return (stream, file);
    }
}
