using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;

namespace BookStorage.Api.Modules;

public static class BooksModule
{
    public static void MapBooks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books")
            .WithTags("Books");

        group.MapGet("/", GetBooks)
            .WithName("GetBooks")
            .Produces<IEnumerable<BookListItemDto>>(200);

        group.MapGet("/{id:guid}", GetBook)
            .WithName("GetBook")
            .Produces<BookDto>(200)
            .Produces(404);

        group.MapPost("/", CreateBook)
            .DisableAntiforgery()
            .WithName("CreateBook")
            .Accepts<CreateBookRequestDto>("multipart/form-data")
            .Produces<CreateBookResponseDto>(200)
            .Produces(400);
    }

    private static async Task<IResult> GetBooks(IBookService bookService, IMapper mapper, CancellationToken ct)
    {
        var books = await bookService.GetAllAsync(ct);
        var booksDto = mapper.Map<IEnumerable<Book>, IEnumerable<BookListItemDto>>(books);
        return Results.Ok(booksDto);
    }

    private static async Task<IResult> GetBook(Guid id, IBookService bookService, IMapper mapper,
        LinkGenerator linkGenerator, HttpContext httpContext, CancellationToken ct)
    {
        var book = await bookService.GetByIdAsync(id, ct);
        if (book == null)
        {
            return Results.NotFound();
        }

        var bookDto = mapper.Map<Book, BookDto>(book);

        if (bookDto.Files != null)
        {
            bookDto.Files = bookDto.Files.Select(file =>
            {
                var url = linkGenerator.GetUriByRouteValues(
                    httpContext, "GetBookFile", new { id = file.Id });
                return file with { DownloadUrl = url };
            }).ToList();
        }

        return Results.Ok(bookDto);
    }

    private static async Task<IResult> CreateBook(
        CreateBookRequestDto request,
        IBookService bookService,
        IMapper mapper,
        CancellationToken ct)
    {
        var newBook = mapper.Map<CreateBookRequestDto, Book>(request);

        var fileType = Path.GetExtension(request.File.FileName)?.TrimStart('.') ?? "";
        if (string.IsNullOrEmpty(fileType))
        {
            return Results.BadRequest("File must have an extension");
        }

        await using var st = request.File.OpenReadStream();
        var book = await bookService.CreateAsync(newBook, st, fileType, ct);
        
        if (book == null)
        {
            return Results.BadRequest("Failed to create book");
        }
        
        var response = new CreateBookResponseDto(book.Id.ToString());
        
        return Results.Ok(response);
    }
}