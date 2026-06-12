using System.Reflection;
using System.Text.Json;

namespace BookStorage.Api.DTOs;

public abstract class BaseBookDto
{
    public string? Id { get; set; }
    public string Title {get; set;} = String.Empty;
}

public class BookListItemDto: BaseBookDto{
    public string? CategoryId { get; set; }
}

public class BookDto : BaseBookDto 
{
    public string? Description {get; set;}
    public string? CategoryId {get; set;}
    public CategoryDto? Category {get; set;}
    public IEnumerable<PersonDto>? Authors {get; set;}
    public IEnumerable<BookFileDto>? Files {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime? UpdatedAt {get; set;}
}

public class CreateBookRequestDto
{
    public required string Title {get; set;}
    public required IFormFile File {get; set;}
    public string? Description {get; set;}
    public CategoryDto? Category {get; set;}
    public IEnumerable<PersonDto>? Authors {get; set;}

    public static async ValueTask<CreateBookRequestDto?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    {
        var form = await httpContext.Request.ReadFormAsync();

        var title = form["title"].FirstOrDefault();
        if (string.IsNullOrEmpty(title))
            return null;

        var file = form.Files.GetFile("file");
        if (file == null)
            return null;

        var description = form["description"].FirstOrDefault();

        CategoryDto? category = null;
        var categoryJson = form["category"].FirstOrDefault();
        if (!string.IsNullOrEmpty(categoryJson))
        {
            category = JsonSerializer.Deserialize<CategoryDto>(categoryJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        IEnumerable<PersonDto>? authors = null;
        var authorsJson = form["authors"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorsJson))
        {
            authors = JsonSerializer.Deserialize<List<PersonDto>>(authorsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return new CreateBookRequestDto
        {
            Title = title,
            File = file,
            Description = description,
            Category = category,
            Authors = authors
        };
    }
}

public record CreateBookResponseDto (string Id);