using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;

namespace BookStorage.Api.Modules;

public static class CategoryModule
{
    public static void MapCategories(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/Categories")
            .WithTags("Categories");
        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .Produces<CategoryDto>(200)
            .Produces(400);
        group.MapGet("/", GetAllCategories)
            .WithName("GetAllCategories")
            .Produces<IEnumerable<CategoryDto>>(200);
        group.MapGet("/Tree", GetAllCategoriesTree)
            .WithName("GetAllCategoriesTree")
            .Produces<IEnumerable<CategoryDto>>(200);
    }

    private static async Task<IResult> GetAllCategories(ICategoryService cs, IMapper mapper, CancellationToken ct = default)
    {
        var categories = await cs.GetAllAsync(ct);
        var categoriesDto = mapper.Map<IEnumerable<Category>, IEnumerable<CategoryDto>>(categories);
        return Results.Ok(categoriesDto);
    }

    private static async Task<IResult> GetAllCategoriesTree(ICategoryService cs, IMapper mapper, CancellationToken ct = default)
    {
        var categories = (await cs.GetAllAsync(ct)).Where(x => x.ParentCategoryId == null);
        var categoriesDto = mapper.Map<IEnumerable<Category>, IEnumerable<CategoryDto>>(categories);
        return Results.Ok(categoriesDto);
    }

    private static async Task<IResult> CreateCategory([FromBody] CreateCategoryRequestDto requestDto, ICategoryService cs, IMapper mapper, CancellationToken ct = default)
    {
       var category = mapper.Map<CreateCategoryRequestDto, Category>(requestDto);
       var result = await cs.CreateAsync(category, ct);
       if (result != null)
       {
          return Results.Ok(mapper.Map<Category, CategoryDto>(result));
       }
       else
       {
          return Results.BadRequest("Failed to create category");
       }
    }
}

