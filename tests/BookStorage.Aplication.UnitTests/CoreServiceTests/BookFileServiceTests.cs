using AutoFixture;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookStorage.Aplication.UnitTests.CoreServiceTests;

public class BookFileServiceTests
{
    private readonly Mock<IBookFileRepository> _bookFileRepoMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly BookFileService _service;

    public BookFileServiceTests()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList().ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _bookFileRepoMock = new Mock<IBookFileRepository>(MockBehavior.Strict);
        _fileStorageMock = new Mock<IFileStorageService>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<BookFileService>>(MockBehavior.Strict);

        uowMock.Setup(u => u.BookFiles).Returns(_bookFileRepoMock.Object);

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _service = new BookFileService(
            uowMock.Object,
            _fileStorageMock.Object,
            loggerMock.Object);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingFileInDbAndStorage_ReturnsStreamAndMetadata()
    {
        // Arrange
        var fileId = Guid.CreateVersion7();
        var bookFile = new BookFile
        {
            Id = fileId,
            FileName = "document.pdf",
            FullFilePath = "books\\document.pdf",
            FileSizeBytes = 1024000,
            FileType = "application/pdf",
            UploadedAt = DateTime.UtcNow
        };

        var expectedStream = new MemoryStream();

        _bookFileRepoMock
            .Setup(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookFile);

        _fileStorageMock
            .Setup(f => f.GetBookAsync(bookFile.FullFilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        // Act
        var (stream, metadata) = await _service.GetByIdAsync(fileId);

        // Assert
        Assert.NotNull(stream);
        Assert.NotNull(metadata);
        Assert.Same(expectedStream, stream);
        Assert.Equal(fileId, metadata.Id);
        Assert.Equal("document.pdf", metadata.FileName);

        _bookFileRepoMock.Verify(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.GetBookAsync(bookFile.FullFilePath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingInDb_ReturnsNullNull()
    {
        // Arrange
        var fileId = Guid.CreateVersion7();

        _bookFileRepoMock
            .Setup(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookFile?)null);

        // Act
        var (stream, metadata) = await _service.GetByIdAsync(fileId);

        // Assert
        Assert.Null(stream);
        Assert.Null(metadata);

        _bookFileRepoMock.Verify(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.GetBookAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingInDbButMissingInStorage_ReturnsNullNull()
    {
        // Arrange
        var fileId = Guid.CreateVersion7();
        var bookFile = new BookFile
        {
            Id = fileId,
            FileName = "missing.pdf",
            FullFilePath = "books\\missing.pdf",
            FileSizeBytes = 500,
            FileType = "application/pdf",
            UploadedAt = DateTime.UtcNow
        };

        _bookFileRepoMock
            .Setup(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookFile);

        _fileStorageMock
            .Setup(f => f.GetBookAsync(bookFile.FullFilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        // Act
        var (stream, metadata) = await _service.GetByIdAsync(fileId);

        // Assert
        Assert.Null(stream);
        Assert.Null(metadata);
    }

    [Fact]
    public async Task GetByIdAsync_UsesFullFilePathWhenAvailable()
    {
        // Arrange
        var fileId = Guid.CreateVersion7();
        var bookFile = new BookFile
        {
            Id = fileId,
            FileName = "file.pdf",
            FullFilePath = "subfolder\\nested\\file.pdf",
            FileSizeBytes = 100,
            UploadedAt = DateTime.UtcNow
        };

        _bookFileRepoMock
            .Setup(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookFile);

        _fileStorageMock
            .Setup(f => f.GetBookAsync("subfolder\\nested\\file.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());

        // Act
        var (stream, metadata) = await _service.GetByIdAsync(fileId);

        // Assert
        Assert.NotNull(stream);
        Assert.NotNull(metadata);

        _fileStorageMock.Verify(
            f => f.GetBookAsync("subfolder\\nested\\file.pdf", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            f => f.GetBookAsync("file.pdf", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_FallsBackToFileNameWhenFullFilePathNull()
    {
        // Arrange
        var fileId = Guid.CreateVersion7();
        var bookFile = new BookFile
        {
            Id = fileId,
            FileName = "rootfile.pdf",
            FullFilePath = null,
            FileSizeBytes = 200,
            UploadedAt = DateTime.UtcNow
        };

        _bookFileRepoMock
            .Setup(r => r.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookFile);

        _fileStorageMock
            .Setup(f => f.GetBookAsync("rootfile.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());

        // Act
        var (stream, metadata) = await _service.GetByIdAsync(fileId);

        // Assert
        Assert.NotNull(stream);
        Assert.NotNull(metadata);

        _fileStorageMock.Verify(
            f => f.GetBookAsync("rootfile.pdf", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion GetByIdAsync
}
