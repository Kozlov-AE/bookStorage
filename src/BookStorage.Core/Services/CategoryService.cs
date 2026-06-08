using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace BookStorage.Core.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
    {
        _uow = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Getting all categories");
        var categories = await _uow.Categories.GetAllAsync(ct);
        _logger.LogInformation("Retrieved {CategoriesCount} categories", categories.Count());
        return categories;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting category with ID: {CategoryId}", id);
        var category = await _uow.Categories.GetByIdAsync(id, ct);
        if (category == null)
        {
            _logger.LogWarning("Category with ID {CategoryId} not found", id);
        }
        return category;
    }

    public async Task<IEnumerable<Category>> GetByNameAsync(string name, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting categories by name: {CategoryName}", name);
        var categories = await _uow.Categories.GetByName(name, ct);
        _logger.LogInformation("Found {CategoriesCount} categories with name: {CategoryName}", categories.Count(), name);
        return categories;
    }

    public async Task<Category?> CreateAsync(Category category, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating category: {CategoryName}", category.Name);
        var existingCategories = (await _uow.Categories.GetByName(category.Name, ct)).ToArray();
        if (existingCategories.Length > 0)
        {
            var existing = existingCategories.FirstOrDefault(c => c.ParentCategoryId == category.ParentCategoryId);
            if (existing != null)
            {
                _logger.LogInformation("Category already exists: {CategoryId} - {CategoryName}", existing.Id, existing.Name);
                return existing;
            }
            return null;
        }
        var cat = await CreateCategory(category, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Category created successfully: {CategoryId} - {CategoryName}", cat.Id, cat.Name);
        return cat;
    }

    public async Task<Category?> UpdateAsync(Guid id, Category category, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId}", id);
        var existing = await _uow.Categories.GetByIdAsync(id, ct);
        if (existing == null)
        {
            _logger.LogWarning("Category with ID {CategoryId} not found for update", id);
            return null;
        }

        existing.Name = category.Name;
        existing.ParentCategoryId = category.ParentCategoryId;
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Category updated successfully: {CategoryId} - {CategoryName}", id, existing.Name);

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting category with ID: {CategoryId}", id);
        var result = await _uow.Categories.DeleteAsync(id, ct);
        if (result)
        {
            _logger.LogInformation("Category deleted successfully: {CategoryId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to delete category with ID: {CategoryId}", id);
        }
        return result;
    }

    private async Task<Category> CreateCategory(Category category, CancellationToken ct = default)
    {
        var cat = await _uow.Categories.AddAsync(category, ct);
        return category;
    }
}