using AutoFixture;
using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using MapsterMapper;
using Xunit;

namespace BookStorage.Aplication.UnitTests.MappingTests;

public class BookFileMappingTests
{
    private readonly Mapper _mapper;

    public BookFileMappingTests()
    {
        _mapper = MapsterTestHelper.GetMapperForTests();
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private BookFile CreateSampleBookFile(
        Guid? id = null,
        string? fileName = null,
        long? fileSizeBytes = null,
        DateTime? uploadedAt = null,
        string? fileType = null)
    {
        return new BookFile
        {
            Id = id ?? Guid.CreateVersion7(),
            FileName = fileName ?? "test.pdf",
            FileSizeBytes = fileSizeBytes ?? 1024000,
            FileType = fileType,
            UploadedAt = uploadedAt ?? DateTime.Now
        };
    }

    [Fact]
    public void Map_BookFile_To_BookFileDto_All_Fields_Mapped_Correctly()
    {
        // Arrange
        var uploadTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var file = CreateSampleBookFile(
            id: Guid.CreateVersion7(),
            fileName: "book.pdf",
            fileSizeBytes: 2048000,
            uploadedAt: uploadTime,
            fileType: "application/pdf"
        );

        // Act
        var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

        // Assert
        Assert.NotNull(fileDto);
        Assert.Equal(file.Id.ToString(), fileDto.Id);
        Assert.Equal("book.pdf", fileDto.FileName);
        Assert.Equal(2048000, fileDto.FileSizeBytes);
        Assert.Equal(uploadTime, fileDto.UploadedAt);
        Assert.Equal("application/pdf", fileDto.FileType);
    }

    [Fact]
    public void Map_BookFile_To_BookFileDto_With_Empty_Id_Should_Ok()
    {
        // Arrange
        var file = CreateSampleBookFile(id: Guid.Empty);

        // Act
        var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

        // Assert
        Assert.NotNull(fileDto);
        Assert.Null(fileDto.Id);
        Assert.NotNull(fileDto.FileName);
    }

    [Fact]
    public void Map_BookFile_To_BookFileDto_With_Null_FileType_Should_Ok()
    {
        // Arrange — fileName is required, but fileType can be null
        var file = CreateSampleBookFile(
            id: Guid.CreateVersion7(),
            fileName: "test.pdf",
            fileType: null
        );

        // Act
        var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

        // Assert
        Assert.NotNull(fileDto);
        Assert.NotNull(fileDto.Id);
        Assert.Equal("test.pdf", fileDto.FileName);
        Assert.Null(fileDto.FileType);
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("ebook.epub")]
    [InlineData("小说.txt")]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData("very_long_filename_with_multiple_dots_and_special_chars-v2.1.0.pdf")]
    public void Map_BookFile_To_BookFileDto_Different_FileNames_Should_Ok(string fileName)
    {
        // Arrange
        var file = CreateSampleBookFile(fileName: fileName);

        // Act
        var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

        // Assert
        Assert.NotNull(fileDto);
        Assert.Equal(fileName, fileDto.FileName);
    }

    [Fact]
    public void Map_BookFile_To_BookFileDto_With_Various_File_Sizes_Should_Ok()
    {
        // Arrange
        var fileSizes = new long[] { 0, 1024, 1024000, 10485760, 1073741824 };

        foreach (var fileSize in fileSizes)
        {
            var file = CreateSampleBookFile(fileSizeBytes: fileSize);

            // Act
            var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

            // Assert
            Assert.NotNull(fileDto);
            Assert.Equal(fileSize, fileDto.FileSizeBytes);
        }
    }
}