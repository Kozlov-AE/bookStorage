namespace BookStorage.Api.DTOs;

public record PublisherDto(
    int Id,
    string Name
);

public record CreatePublisherRequest(string Name);

public record UpdatePublisherRequest(string Name);
