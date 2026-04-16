namespace BookStorage.Api.DTOs;

public record CountryDto(
    int Id,
    string Name
);

public record CreateCountryRequest(string Name);

public record UpdateCountryRequest(string Name);
