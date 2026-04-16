namespace BookStorage.Api.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    int? ParentCategoryId,
    IEnumerable<CategoryDto>? SubCategories
);

public record CreateCategoryRequest(
    string Name,
    int? ParentCategoryId
);

public record UpdateCategoryRequest(
    string Name,
    int? ParentCategoryId
);
