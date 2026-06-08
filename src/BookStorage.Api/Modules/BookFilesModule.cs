using BookStorage.Core.Interfaces.Application;

namespace BookStorage.Api.Modules;

public static class BookFilesModule
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pdf"] = "application/pdf",
        ["epub"] = "application/epub+zip",
        ["fb2"] = "application/x-fictionbook+xml",
        ["djvu"] = "image/vnd.djvu",
        ["txt"] = "text/plain",
        ["doc"] = "application/msword",
        ["docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ["mobi"] = "application/x-mobipocket-ebook",
        ["azw3"] = "application/vnd.amazon.ebook",
    };

    public static void MapBookFiles(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files").WithTags("Files");
        group.MapGet("/{id:guid}", GetBookFile)
            .WithName("GetBookFile");
    }

    private static async Task<IResult> GetBookFile(Guid id, IBookFileService service)
    {
        var (stream, file) = await service.GetByIdAsync(id);
        if (stream == null || file == null)
            return Results.NotFound();

        var contentType = MimeTypes.GetValueOrDefault(file.FileType ?? "", "application/octet-stream");
        var downloadName = $"{file.FileName}.{file.FileType}";

        return Results.File(stream, contentType, downloadName);
    }
}
