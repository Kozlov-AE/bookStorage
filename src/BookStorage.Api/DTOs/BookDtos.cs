namespace BookStorage.Api.DTOs;

public record BookDto(
    Guid Id,
    string Title,
    string? Description,
    string? ISBN,
    int Pages,
    int Year,
    DateTime CreatedAt,
    PublisherDto? Publisher,
    CategoryDto? Category,
    LanguageDto? Language,
    CountryDto? Country,
    IEnumerable<GenreDto>? Genres,
    IEnumerable<PersonDto>? Authors,
    IEnumerable<PersonDto>? Translators,
    IEnumerable<BookFileDto>? Files
);

public record BookListDto(
    Guid Id,
    string Title,
    int Year,
    string? ISBN,
    IEnumerable<PersonDto>? Authors
);

public record CreateBookRequest(
    string Title,
    string? Description,
    string? ISBN,
    int Pages,
    int Year,
    Guid? PublisherId,
    Guid? CategoryId,
    Guid? LanguageId,
    Guid? CountryId,
    IEnumerable<Guid>? GenreIds,
    IEnumerable<Guid>? AuthorIds,
    IEnumerable<Guid>? TranslatorIds
);

public record UpdateBookRequest(
    string Title,
    string? Description,
    string? ISBN,
    int Pages,
    int Year,
    Guid? PublisherId,
    Guid? CategoryId,
    Guid? LanguageId,
    Guid? CountryId,
    IEnumerable<Guid>? GenreIds,
    IEnumerable<Guid>? AuthorIds,
    IEnumerable<Guid>? TranslatorIds
);