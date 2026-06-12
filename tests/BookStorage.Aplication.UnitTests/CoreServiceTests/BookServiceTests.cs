using AutoFixture;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookStorage.Aplication.UnitTests.CoreServiceTests;

public class BookServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IBookRepository> _bookRepoMock;
    private readonly Mock<IPersonRepository> _personRepoMock;
    private readonly Mock<ICategoryRepository> _catRepoMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly BookService _service;

    public BookServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<DateOnly>(
            composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));

        _uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _bookRepoMock = new Mock<IBookRepository>(MockBehavior.Strict);
        _personRepoMock = new Mock<IPersonRepository>(MockBehavior.Strict);
        _catRepoMock = new Mock<ICategoryRepository>(MockBehavior.Strict);
        _fileStorageMock = new Mock<IFileStorageService>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<BookService>>(MockBehavior.Strict);

        _uowMock.Setup(u => u.Books).Returns(_bookRepoMock.Object);
        _uowMock.Setup(u => u.Persons).Returns(_personRepoMock.Object);
        _uowMock.Setup(u => u.Categories).Returns(_catRepoMock.Object);

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _service = new BookService(
            _uowMock.Object,
            _fileStorageMock.Object,
            loggerMock.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        // Arrange
        var books = _fixture.CreateMany<Book>(3).ToList();

        _bookRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        _bookRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Arrange
        _bookRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion GetAllAsync

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsBookWithDetails()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var book = _fixture.Build<Book>()
            .With(b => b.Id, bookId)
            .Create();

        _bookRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _service.GetByIdAsync(bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookId, result.Id);
        _bookRepoMock.Verify(
            r => r.GetByIdWithDetailsAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        _bookRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _service.GetByIdAsync(bookId);

        // Assert
        Assert.Null(result);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_NewBookNoAuthorsNoCategory_SavesAndReturns()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Description = "Test Description",
            Authors = new List<Person>(),
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        var savedFile = new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = "test.pdf",
            FileSizeBytes = 1024,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(It.IsAny<IEnumerable<Person>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _fileStorageMock
            .Setup(f => f.SaveBookAsync(It.IsAny<Stream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFile);

        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _uowMock
            .Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(book, stream, fileType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
        Assert.Single(result.Files);
        Assert.Equal(savedFile.Id, result.Files.First().Id);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.True(result.CreatedAt > DateTime.UtcNow.AddMinutes(-1));

        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _personRepoMock.Verify(r => r.AddAsync(It.IsAny<IEnumerable<Person>>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.SaveBookAsync(It.IsAny<MemoryStream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
        _bookRepoMock.Verify(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NewBookWithNewAuthors_CreatesAndMatchesIds()
    {
        // Arrange
        var author1 = new Person { Id = Guid.Empty, FullName = "Alice" };
        var author2 = new Person { Id = Guid.Empty, FullName = "Bob" };

        var savedAuthor1 = new Person { Id = Guid.CreateVersion7(), FullName = "Alice" };
        var savedAuthor2 = new Person { Id = Guid.CreateVersion7(), FullName = "Bob" };

        var book = new Book
        {
            Title = "Co-authored Book",
            Authors = new List<Person> { author1, author2 },
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        var savedFile = new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = "coauthored.pdf",
            FileSizeBytes = 2048,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(
                It.IsAny<IEnumerable<Person>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([savedAuthor1, savedAuthor2]);

        _fileStorageMock
            .Setup(f => f.SaveBookAsync(It.IsAny<MemoryStream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFile);

        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _uowMock
            .Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(book, stream, fileType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Authors.Count);

        var alice = result.Authors.First(a => a.FullName == "Alice");
        var bob = result.Authors.First(a => a.FullName == "Bob");
        Assert.NotEqual(Guid.Empty, alice.Id);
        Assert.NotEqual(Guid.Empty, bob.Id);
        Assert.Equal(savedAuthor1.Id, alice.Id);
        Assert.Equal(savedAuthor2.Id, bob.Id);
    }

    [Fact]
    public async Task CreateAsync_NewBookWithMixedAuthors_OnlyNewOnesPassedToRepository()
    {
        // Arrange
        var existingId = Guid.CreateVersion7();
        var existingAuthor = new Person { Id = existingId, FullName = "Existing Author" };
        var newAuthor = new Person { Id = Guid.Empty, FullName = "New Author" };

        var savedNewAuthor = new Person { Id = Guid.CreateVersion7(), FullName = "New Author" };

        var book = new Book
        {
            Title = "Mixed Authors Book",
            Authors = new List<Person> { existingAuthor, newAuthor },
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        var savedFile = new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = "mixed.pdf",
            FileSizeBytes = 1024,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(
                It.IsAny<IEnumerable<Person>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([savedNewAuthor]);

        _fileStorageMock
            .Setup(f => f.SaveBookAsync(It.IsAny<MemoryStream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFile);

        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _uowMock
            .Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(book, stream, fileType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Authors.Count);

        var existing = result.Authors.First(a => a.FullName == "Existing Author");
        Assert.Equal(existingId, existing.Id);

        var newRes = result.Authors.First(a => a.FullName == "New Author");
        Assert.NotEqual(Guid.Empty, newRes.Id);
        Assert.Equal(savedNewAuthor.Id, newRes.Id);

        _personRepoMock.Verify(r => r.AddAsync(
            It.IsAny<IEnumerable<Person>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NewBookWithExistingCategoryId_ValidatesAndUses()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var category = new Category { Id = categoryId, Name = "Fiction" };

        var book = new Book
        {
            Title = "Categorized Book",
            CategoryId = categoryId,
            Authors = new List<Person>(),
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        var savedFile = new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = "cat.pdf",
            FileSizeBytes = 512,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(It.IsAny<IEnumerable<Person>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _fileStorageMock
            .Setup(f => f.SaveBookAsync(It.IsAny<MemoryStream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFile);

        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _uowMock
            .Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(book, stream, fileType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.CategoryId);

        _catRepoMock.Verify(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NewBookWithInvalidCategoryId_RollsBackAndReturnsNull()
    {
        // Arrange
        var book = new Book
        {
            Title = "Bad Category Book",
            CategoryId = Guid.CreateVersion7(),
            Authors = new List<Person>(),
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(It.IsAny<IEnumerable<Person>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _catRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        _uowMock
            .Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(book, stream, fileType);

        // Assert
        Assert.Null(result);

        _catRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _bookRepoMock.Verify(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
        _fileStorageMock.Verify(f => f.SaveBookAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NewBookWithCategoryNav_InsertsNewCategory()
    {
        // Arrange
        var category = new Category { Id = Guid.Empty, Name = "New Category" };

        var book = new Book
        {
            Title = "Nav Category Book",
            Category = category,
            Authors = new List<Person>(),
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        var savedFile = new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = "navcat.pdf",
            FileSizeBytes = 1024,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(It.IsAny<IEnumerable<Person>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _catRepoMock
            .Setup(r => r.AddAsync(It.Is<Category>(c => c.Name == "New Category"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _fileStorageMock
            .Setup(f => f.SaveBookAsync(It.IsAny<MemoryStream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFile);

        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _uowMock
            .Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(book, stream, fileType);

        // Assert
        Assert.NotNull(result);

        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Null(result.CategoryId);
        _bookRepoMock.Verify(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSaveChangesFails_RollsBackAndRethrows()
    {
        // Arrange
        var book = new Book
        {
            Title = "Failing Book",
            Authors = new List<Person>(),
            Files = new List<BookFile>()
        };

        using var stream = new MemoryStream();
        const string fileType = "application/pdf";

        var savedFile = new BookFile
        {
            Id = Guid.CreateVersion7(),
            FileName = "fail.pdf",
            FileSizeBytes = 100,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };

        _uowMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _personRepoMock
            .Setup(r => r.AddAsync(It.IsAny<IEnumerable<Person>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _fileStorageMock
            .Setup(f => f.SaveBookAsync(It.IsAny<MemoryStream>(), fileType, It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFile);

        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB failure"));

        _uowMock
            .Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(book, stream, fileType));

        Assert.Equal("DB failure", ex.Message);

        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ExistingId_UpdatesFieldsAndReturns()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var existing = _fixture.Build<Book>()
            .With(b => b.Id, bookId)
            .With(b => b.Title, "Old Title")
            .With(b => b.Description, "Old Description")
            .With(b => b.Authors, new List<Person>())
            .With(b => b.Files, new List<BookFile>())
            .Create();

        var updateDto = new Book
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        _bookRepoMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(bookId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt <= DateTime.UtcNow);

        _bookRepoMock.Verify(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepoMock.Verify(r => r.Update(It.IsAny<Book>()), Times.Never);
        _bookRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        _bookRepoMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _service.UpdateAsync(bookId, new Book { Title = "Anything" });

        // Assert
        Assert.Null(result);
        _bookRepoMock.Verify(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepoMock.Verify(r => r.Update(It.IsAny<Book>()), Times.Never);
        _bookRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesAndReturnsTrue()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        _bookRepoMock
            .Setup(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteAsync(bookId);

        // Assert
        Assert.True(result);
        _bookRepoMock.Verify(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();

        _bookRepoMock
            .Setup(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(bookId);

        // Assert
        Assert.False(result);
        _bookRepoMock.Verify(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion DeleteAsync
}
