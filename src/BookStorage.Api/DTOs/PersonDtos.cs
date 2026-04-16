namespace BookStorage.Api.DTOs;

public record PersonDto(
    int Id,
    string Name,
    string? Bio,
    int? BirthYear,
    int? DeathYear
);

public record CreatePersonRequest(
    string Name,
    string? Bio,
    int? BirthYear,
    int? DeathYear
);

public record UpdatePersonRequest(
    string Name,
    string? Bio,
    int? BirthYear,
    int? DeathYear
);
