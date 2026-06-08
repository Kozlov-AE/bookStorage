using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;
using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace BookStorage.Core.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fs;
    private readonly ILogger<BookService> _logger;

    public BookService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, ILogger<BookService> logger)
    {
        _uow = unitOfWork;
        _fs = fileStorageService;
        _logger = logger;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Getting all books");
        var books = await _uow.Books.GetAllAsync(ct);
        _logger.LogInformation("Retrieved {BooksCount} books", books.Count());
        return books;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting book with ID: {BookId}", id);
        var book = await _uow.Books.GetByIdWithDetailsAsync(id, ct);
        if (book == null)
        {
            _logger.LogWarning("Book with ID {BookId} not found", id);
        }

        return book;
    }

    public async Task<Book?> CreateAsync(Book book, Stream fileStream, string fileType, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating book: {BookTitle} with {AuthorsCount} authors",
            book.Title,
            book.Authors?.Count ?? 0);
        try
        {
            await _uow.BeginTransactionAsync(ct);
            _logger.LogDebug("Transaction started for book creation: {BookTitle}", book.Title);

            if (book.Authors != null)
            {
                var newAuthors = await _uow.Persons.AddAsync(book.Authors.Where(a => a.Id == Guid.Empty), ct);
                _logger.LogDebug(
                    "Added {NewAuthorsCount} new authors for book: {BookTitle}",
                    newAuthors.Count(),
                    book.Title);

                book.Authors = book.Authors.Select(x =>
                {
                    if (x.Id == Guid.Empty)
                    {
                        var author = newAuthors.FirstOrDefault(a => a.FullName.Equals(
                            x.FullName,
                            StringComparison.InvariantCultureIgnoreCase));
                        if (author != null)
                        {
                            x.Id = author.Id;
                        }
                    }

                    return x;
                }).ToList();
            }

            Category? existingCat = null;
            if (book.CategoryId.HasValue && book.CategoryId != Guid.Empty)
            {
                existingCat = await _uow.Categories.GetByIdAsync(book.CategoryId.Value, ct);
                if (existingCat == null)
                {
                    _logger.LogError("Category with ID {CategoryId} not found", book.CategoryId);
                    await _uow.RollbackTransactionAsync(ct);
                    return null;
                }
            }

            if (existingCat is null && book.Category != null)
            { 
                await _uow.Categories.AddAsync(book.Category, ct);
            }
            
            book.Category = null;

            var file = await _fs.SaveBookAsync(fileStream, fileType, book, ct);
            _logger.LogDebug(
                "File saved for book: {BookTitle}, FileType: {FileType}, Size: {FileSizeBytes}",
                book.Title,
                fileType,
                file.FileSizeBytes);
            book.Files.Add(file);

            book.CreatedAt = DateTime.UtcNow;
            await _uow.Books.AddAsync(book, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Book created successfully: {BookId} - {BookTitle}", book.Id, book.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book: {BookTitle}", book.Title);
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        return book;
    }

    public async Task<Book?> UpdateAsync(Guid id, Book book, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating book with ID: {BookId}", id);
        var existing = await _uow.Books.GetByIdAsync(id, ct);
        if (existing == null)
        {
            _logger.LogWarning("Book with ID {BookId} not found for update", id);
            return null;
        }

        existing.Title = book.Title;
        existing.Description = book.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Book updated successfully: {BookId} - {BookTitle}", id, existing.Title);

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting book with ID: {BookId}", id);
        var result = await _uow.Books.DeleteAsync(id, ct);
        if (result)
        {
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation("Book deleted successfully: {BookId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to delete book with ID: {BookId}", id);
        }

        return result;
    }
}