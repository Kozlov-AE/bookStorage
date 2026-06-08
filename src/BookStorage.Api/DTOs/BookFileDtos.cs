namespace BookStorage.Api.DTOs;

public record BookFileDto(
    string Id,
    string FileName,
    long FileSizeBytes,
    DateTime UploadedAt,
    string? FileType,
    string? DownloadUrl
);

public record CreateBookFileRequest(
    string FileName
);
