namespace BookStorage.Api.DTOs;

public record BookFileDto(
    int Id,
    string Format,
    string Hash,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    DateTime UploadedAt
);

public record CreateBookFileRequest(
    string Format,
    string FileName
);
