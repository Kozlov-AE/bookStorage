namespace BookStorage.Api.DTOs;

public record LanguageDto(
    int Id,
    string Name,
    string Code
);

public record CreateLanguageRequest(
    string Name,
    string Code
);

public record UpdateLanguageRequest(
    string Name,
    string Code
);
