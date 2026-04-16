namespace BookStorage.Api.DTOs;

public record GenreDto(
    int Id,
    string Name
);

public record CreateGenreRequest(string Name);

public record UpdateGenreRequest(string Name);
