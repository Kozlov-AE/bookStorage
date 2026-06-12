using AutoFixture;
using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace BookStorage.Aplication.UnitTests.MappingTests;

public class BookMappingTests
{
    private readonly Mapper _mapper;
    private readonly Fixture _fixture;

    public BookMappingTests()
    {
        _mapper = MapsterTestHelper.GetMapperForTests();
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(date => DateOnly.FromDateTime(date)));
    }

    private Book CreateSampleBook(
        Guid? id = null, 
        string? title = null, 
        string? description = null,
        Category? category = null,
        Guid? categoryId = null,
        List<Person>? authors = null,
        List<BookFile>? files = null,
        DateTime? updatedAt = null)
    {
        var book = _fixture.Create<Book>();
        book.Id = id ?? book.Id;
        book.Title = title ?? book.Title;
        book.Description = description ?? book.Description;
        book.Category = category ?? book.Category;
        book.CategoryId = categoryId ?? book.CategoryId;
        book.Authors = authors ?? book.Authors;
        book.Files = files ?? book.Files;
        book.UpdatedAt = updatedAt;
        
        return book;
    }

    private Category CreateSampleCategory(Guid? id = null, string? name = null)
    {
        return new Category
        {
            Id = id ?? Guid.CreateVersion7(),
            Name = name ?? "Fiction"
        };
    }

    private Person CreateSamplePerson(Guid? id = null, string? fullName = null, DateOnly? birthday = null)
    {
        return new Person
        {
            Id = id ?? Guid.CreateVersion7(),
            FullName = fullName ?? "Test Author",
            Birthday = birthday ?? new DateOnly(1990, 5, 15)
        };
    }

    private BookFile CreateSampleBookFile(Guid? id = null)
    {
        return new BookFile
        {
            Id = id ?? Guid.CreateVersion7(),
            FileName = "test.pdf",
            FileSizeBytes = 1024000,
            UploadedAt = DateTime.Now
        };
    }

    [Fact]
    public void Map_Book_To_BookDto_Should_Ok()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();
        var category = CreateSampleCategory(categoryId, "Fiction");

        var author1 = CreateSamplePerson(Guid.CreateVersion7(), "Author One", new DateOnly(1970, 1, 1));
        var author2 = CreateSamplePerson(Guid.CreateVersion7(), "Author Two", new DateOnly(1980, 2, 2));
        var authors = new List<Person> { author1, author2 };

        var bookFile = CreateSampleBookFile(Guid.CreateVersion7());
        var files = new List<BookFile> { bookFile };

        var book = CreateSampleBook(
            id: bookId,
            title: "The Test Book",
            description: "A test book for testing",
            category: category,
            categoryId: categoryId,
            authors: authors,
            files: files
        );

        // Act
        var bookDto = _mapper.Map<Book, BookDto>(book);

        // Assert
        Assert.NotNull(bookDto);
        Assert.Equal(bookId.ToString(), bookDto.Id);
        Assert.Equal("The Test Book", bookDto.Title);
        Assert.Equal("A test book for testing", bookDto.Description);

        Assert.NotNull(bookDto.Category);
        Assert.Equal(categoryId.ToString(), bookDto.Category.Id);
        Assert.Equal("Fiction", bookDto.Category.Name);

        Assert.NotNull(bookDto.Authors);
        Assert.Equal(2, bookDto.Authors.Count());
        Assert.Contains(bookDto.Authors, a => a.FullName == "Author One");
        Assert.Contains(bookDto.Authors, a => a.FullName == "Author Two");

        Assert.NotNull(bookDto.Files);
        Assert.Single(bookDto.Files);
        Assert.Equal(bookFile.FileName, bookDto.Files.First().FileName);

       
        Assert.Equal(categoryId.ToString(), bookDto.CategoryId);

        Assert.Null(bookDto.UpdatedAt);
    }

    [Fact]
    public void Map_Book_To_BookDto_With_Empty_Collections_Should_Ok()
    {
        // Arrange
        var book = CreateSampleBook(
            authors: new List<Person>(),
            files: new List<BookFile>()
        );

        // Act
        var bookDto = _mapper.Map<Book, BookDto>(book);

        // Assert
        Assert.NotNull(bookDto);
        Assert.NotNull(bookDto.Authors);
        Assert.Empty(bookDto.Authors);
        Assert.NotNull(bookDto.Files);
        Assert.Empty(bookDto.Files);
    }

    [Fact]
    public void Map_CreateBookRequestDto_To_Book_Should_Ok()
    {
       
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var categoryDto = new CategoryDto("Test Category");

        var authorDto1 = new PersonDto(Guid.CreateVersion7().ToString(), "Test Author 1", new DateOnly(1990, 1, 1));
        var authorDto2 = new PersonDto(null, "New Author 2", null);
        var authorDtos = new List<PersonDto> { authorDto1, authorDto2 };

        var requestDto = new CreateBookRequestDto
        {
            Title = "New Test Book",
            File = fileMock.Object,
            Description = "A brand new book",
            Category = categoryDto,
            Authors = authorDtos
        };

        // Act
        var book = _mapper.Map<CreateBookRequestDto, Book>(requestDto);

        // Assert
        Assert.NotNull(book);
        Assert.Equal("New Test Book", book.Title);
        Assert.Equal("A brand new book", book.Description);
        Assert.Null(book.CategoryId);
        Assert.NotNull(book.Category);
        Assert.Equal(categoryDto.Name, book.Category.Name);

        Assert.True(DateTime.UtcNow.AddMinutes(-1) <= book.CreatedAt);
        Assert.True(book.CreatedAt <= DateTime.UtcNow);

        Assert.NotNull(book.Authors);
        Assert.Equal(2, book.Authors.Count);
        Assert.Contains(book.Authors, a => a.FullName == "Test Author 1");
        Assert.Contains(book.Authors, a => a.FullName == "New Author 2");

        var newAuthor = book.Authors.First(a => a.FullName == "New Author 2");
        Assert.Equal(Guid.Empty, newAuthor.Id);
    }

    [Fact]
    public void Map_CreateBookRequestDto_To_Book_With_Null_Category_Should_Ok()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var requestDto = new CreateBookRequestDto
        {
            Title = "Book Without Category",
            File = fileMock.Object,
            Description = "Description",
            Category = null,
            Authors = new List<PersonDto>()
        };

        // Act
        var book = _mapper.Map<CreateBookRequestDto, Book>(requestDto);

        // Assert
        Assert.NotNull(book);
        Assert.Equal("Book Without Category", book.Title);
        Assert.Null(book.CategoryId);
        Assert.NotNull(book.Authors);
        Assert.Empty(book.Authors);
    }

    [Fact]
    public void Map_CreateBookRequestDto_To_Book_With_Null_Authors_Should_Ok()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var requestDto = new CreateBookRequestDto
        {
            Title = "Book Without Authors",
            File = fileMock.Object,
            Description = null,
            Category = null,
            Authors = null
        };

        // Act
        var book = _mapper.Map<CreateBookRequestDto, Book>(requestDto);

        // Assert
        Assert.NotNull(book);
        Assert.Equal("Book Without Authors", book.Title);
        Assert.NotNull(book.Authors);
        Assert.Empty(book.Authors);
    }

    [Theory]
    [InlineData("Fiction Book")]
    [InlineData("Non-Fiction: How to Code")]
    [InlineData("空ノード - Empty Node")]
    [InlineData("")]
    [InlineData(" ")]
    public void Map_CreateBookRequestDto_To_Book_Different_Titles_Should_Ok(string title)
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var requestDto = new CreateBookRequestDto
        {
            Title = title,
            File = fileMock.Object,
            Description = "Test description",
            Category = null,
            Authors = new List<PersonDto>()
        };

        // Act
        var book = _mapper.Map<CreateBookRequestDto, Book>(requestDto);

        // Assert
        Assert.NotNull(book);
        Assert.Equal(title, book.Title);
        Assert.Equal("Test description", book.Description);
    }

    [Fact]
    public void Map_Book_To_BookDto_With_UpdatedAt_Should_Map()
    {
        // Arrange
        var updateTime = new DateTime(2025, 12, 1, 14, 30, 0, DateTimeKind.Utc);
        var book = CreateSampleBook(updatedAt: updateTime);

        // Act
        var bookDto = _mapper.Map<Book, BookDto>(book);

        // Assert
        Assert.NotNull(bookDto);
        Assert.Equal(updateTime, bookDto.UpdatedAt);
    }

    [Fact]
    public void Map_Book_To_BookDto_Empty_Id_Should_Null_Id()
    {
        // Arrange
        var book = CreateSampleBook(id: Guid.Empty);

        // Act
        var bookDto = _mapper.Map<Book, BookDto>(book);

        // Assert
        Assert.NotNull(bookDto);
        Assert.Null(bookDto.Id);
    }

    [Fact]
    public void Map_Book_To_BookListItemDto_Should_Ok()
    {
        var book = CreateSampleBook();

        // Act
        var bookListItemDto = _mapper.Map<Book, BookListItemDto>(book);

        // Assert
        Assert.NotNull(bookListItemDto);
        Assert.Equal(book.Id.ToString(), bookListItemDto.Id);
        Assert.Equal(book.Title, bookListItemDto.Title);
        Assert.Equal(book.CategoryId.ToString(), bookListItemDto.CategoryId);
    }

    [Fact]
    public void Map_Book_To_BookListItemDto_Empty_Id_Should_Null_Id()
    {
        // Arrange
        var book = CreateSampleBook(id: Guid.Empty);
        book.CategoryId = null;

        // Act
        var bookListItemDto = _mapper.Map<Book, BookListItemDto>(book);

        // Assert
        Assert.NotNull(bookListItemDto);
        Assert.Null(bookListItemDto.Id);
        Assert.Null(bookListItemDto.CategoryId);
    }

    [Fact]
    public void Map_Book_To_BookListItemDto_With_Null_Category_Should_Ok()
    {
        // Arrange
        var book = CreateSampleBook();
        book.Category = null;
        book.CategoryId = null;

        // Act
        var bookListItemDto = _mapper.Map<Book, BookListItemDto>(book);

        // Assert
        Assert.NotNull(bookListItemDto);
        Assert.Null(bookListItemDto.CategoryId);
    }
}