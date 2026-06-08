using System.ComponentModel.DataAnnotations;

namespace BookStorage.Api.DTOs;

public record PersonDto(
    string? Id,
    [Required] string FullName,
    DateOnly? Birthday
);
