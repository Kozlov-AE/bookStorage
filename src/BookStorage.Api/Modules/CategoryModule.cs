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
            .Produces<CategoryDto>()
            .Produces(409)
            .Produces(400);
        group.MapGet("/", GetAllCategories)
            .WithName("GetAllCategories")
            .Produces<IEnumerable<CategoryDto>>();
        group.MapGet("/Tree", GetAllCategoriesTree)
            .WithName("GetAllCategoriesTree")
            .Produces<IEnumerable<CategoryDto>>();
        group.MapPut("/{id:guid}", UpdateCategory)
            .WithName("UpdateCategory")
            .Produces<CategoryDto>()
            .Produces(409)
            .Produces(400);
        group.MapDelete("/{id:guid}", DeleteCategory)
            .WithName("DeleteCategory")
            .Produces(200)
            .Produces(404)
            .Produces(409);
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
        if (result.IsSuccess)
        {
            return Results.Ok(mapper.Map<Category, CategoryDto>(result.Value!));
        }
        
        return result.ResultCode switch
        {
            ResultCodes.CategorySameName => Results.Conflict(result.Error),
            _ => Results.BadRequest("Failed to create category")
        };
    }

    private static async Task<IResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequestDto requestDto, ICategoryService cs, IMapper mapper, CancellationToken ct = default)
    {
        var category = mapper.Map<UpdateCategoryRequestDto, Category>(requestDto);
        var result = await cs.UpdateAsync(id, category, ct);
        if (result.IsSuccess)
        {
            return Results.Ok(mapper.Map<Category, CategoryDto>(result.Value!));
        }

        return result.ResultCode switch
        {
            ResultCodes.CategorySameName => Results.Conflict(result.Error),
            ResultCodes.NotFound => Results.NotFound(),
            _ => Results.BadRequest("Failed to update category")
        };
    }

    private static async Task<IResult> DeleteCategory(Guid id, ICategoryService cs, CancellationToken ct = default)
    {
        var result = await cs.DeleteAsync(id, ct);
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return result.ResultCode switch
        {
            ResultCodes.CategoryRemovingNoAllowedBySubCategories => Results.Conflict(result.Error),
            ResultCodes.NotFound => Results.NotFound(),
            _ => Results.BadRequest("Failed to delete category")
        };
    }
}
