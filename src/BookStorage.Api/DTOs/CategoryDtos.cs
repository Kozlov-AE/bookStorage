namespace BookStorage.Api.DTOs;

public class CategoryDto
{
    public string? Id { get; set; }
    public string Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public IEnumerable<CategoryDto>? SubCategories { get; set; }

    public CategoryDto(string name)
    {
        Name = name;
    }
}

public class CreateCategoryRequestDto
{
    public string Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public CreateCategoryRequestDto(string name)
    {
        Name = name;
    }
}

public class UpdateCategoryRequestDto
{
    public string Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public UpdateCategoryRequestDto(string name)
    {
        Name = name;
    }
}
