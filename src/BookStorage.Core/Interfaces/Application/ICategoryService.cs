using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Application;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Category>> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Result<Category>> CreateAsync(Category category, CancellationToken ct = default);
    Task<Result<Category>> UpdateAsync(Guid id, Category category, CancellationToken ct = default);
    Task<Result<Category>> DeleteAsync(Guid id, CancellationToken ct = default);
}