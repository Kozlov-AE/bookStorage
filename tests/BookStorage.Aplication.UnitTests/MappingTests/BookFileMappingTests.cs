using AutoFixture;
using AutoFixture.Xunit3;
using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using MapsterMapper;
using Xunit;

namespace BookStorage.Aplication.UnitTests.MappingTests;

public class BookFileMappingTests
{
    private readonly Mapper _mapper;
    private readonly Fixture _fixture;

    public BookFileMappingTests()
    {
        _mapper = MapsterTestHelper.GetMapperForTests();
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
    public void Map_BookFile_To_BookFileDto_Should_Ok()
    {
        // Arrange
        var file = CreateSampleBookFile(
            id: Guid.CreateVersion7(),
            fileName: "book.pdf",
            fileSizeBytes: 2048000,
            uploadedAt: DateTime.Now
        );

        // Act
        var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

        // Assert
        Assert.NotNull(fileDto);
        Assert.Equal(file.Id.ToString(), fileDto.Id);
        // Примечание: в профиле BookFileToBookFileDto маппятся только Id, другие поля через Mapster defaults
        // Но проверим результаты маппинга из тестовых данных
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
        Assert.Null(fileDto.Id); // Guid.Empty может конвертироваться в null
    }

    [Fact]
    public void Map_BookFile_To_BookFileDto_With_Null_Properties_Should_Ok()
    {
        // Arrange
        var file = CreateSampleBookFile(
            id: Guid.CreateVersion7(),
            fileName: null,
            fileType: null
        );

        // Act
        var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

        // Assert
        Assert.NotNull(fileDto);
        Assert.NotNull(fileDto.Id);
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
        Assert.NotNull(fileDto.Id);
    }

    [Fact]
    public void Map_BookFile_To_BookFileDto_With_Various_File_Sizes_Should_Ok()
    {
        // Arrange
        var fileSizes = new long[] { 0, 1024, 1024000, 10485760, 1073741824 }; // от 0 до 1GB

        foreach (var fileSize in fileSizes)
        {
            var file = CreateSampleBookFile(fileSizeBytes: fileSize);

            // Act
            var fileDto = _mapper.Map<BookFile, BookFileDto>(file);

            // Assert
            Assert.NotNull(fileDto);
            Assert.NotNull(fileDto.Id);
        }
    }
}