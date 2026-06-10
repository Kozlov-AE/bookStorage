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
            .Produces(409)
            .Produces(500);
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

        return Results.BadRequest("Failed to create category");
    }

    private static async Task<IResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequestDto requestDto, ICategoryService cs, IMapper mapper, CancellationToken ct = default)
    {
        var category = mapper.Map<UpdateCategoryRequestDto, Category>(requestDto);
        var result = await cs.UpdateAsync(id, category, ct);
        switch (result.ResultCode)
        {
            case ResultCodes.Success:
                return result.Value == null
                    ? Results.BadRequest()
                    : Results.Ok(mapper.Map<Category, CategoryDto>(result.Value));
            case ResultCodes.NotFound:
                return Results.NotFound();
            case ResultCodes.CategorySameName:
            case ResultCodes.InvalidInput:
                return Results.Conflict(result.Error);
            case ResultCodes.InnerError:
            default:
                return Results.InternalServerError("Inner error: Failed to update category");
        }
    }

    private static async Task<IResult> DeleteCategory(Guid id, ICategoryService cs, CancellationToken ct = default)
    {
        var result = await cs.DeleteAsync(id, ct);
        switch (result.ResultCode)
        {
            case ResultCodes.Success:
                return Results.Ok();
            case ResultCodes.NotFound:
                return Results.NotFound();
            case ResultCodes.CategoryRemovingNoAllowedBySubCategories:
                return Results.Conflict(result.Error);
            case ResultCodes.InnerError:
            default:
                return Results.BadRequest("Inner error: Failed to delete category");
        }
    }
}
