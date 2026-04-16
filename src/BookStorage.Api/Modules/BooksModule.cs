using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookStorage.Api.Modules;

public static class BooksModule
{
    public static void MapBooks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books")
            .WithTags("Books");

        group.MapGet("/", GetBooks)
            .WithName("GetBooks");
        group.MapGet("/{id:guid}", GetBook)
            .WithName("GetBook");
        group.MapPost("/", CreateBook)
            .DisableAntiforgery()
            .WithName("CreateBook");
        group.MapPut("/{id:guid}", UpdateBook)
            .WithName("UpdateBook");
        group.MapDelete("/{id:guid}", DeleteBook)
            .WithName("DeleteBook");
        group.MapPost("/upload", UploadBook)
            .DisableAntiforgery()
            .WithName("UploadBook");
        group.MapGet("/download/{fileName}", DownloadBook)
            .WithName("DownloadBook");
    }

    private static async Task<IResult> GetBooks(CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.Ok(Array.Empty<BookDto>());
    }

    private static async Task<IResult> GetBook(Guid id, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.NotFound(new { message = "Book not found" });
    }

    private static async Task<IResult> CreateBook(
        [FromBody] CreateBookRequest request,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.Ok(new BookDto(
            Guid.Empty, request.Title, request.Description, request.ISBN,
            request.Pages, request.Year, DateTime.UtcNow,
            null, null, null, null, [], [], [], []));
    }

    private static async Task<IResult> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.Ok(new BookDto(
            id, request.Title, request.Description, request.ISBN,
            request.Pages, request.Year, DateTime.UtcNow,
            null, null, null, null, [], [], [], []));
    }

    private static async Task<IResult> DeleteBook(Guid id, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.NoContent();
    }

    private static async Task<IResult> UploadBook(
        IFormFile file,
        [FromForm] string title,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Results.BadRequest("File is empty");

        if (file.Length > 100 * 1024 * 1024)
            return Results.BadRequest("File too large (max 100 MB)");

        await Task.CompletedTask;
        return Results.Ok(new { message = "Upload placeholder - implement with EF Core", title });
    }

    private static async Task<IResult> DownloadBook(
        string fileName,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.NotFound(new { message = "Book not found", fileName });
    }
}
