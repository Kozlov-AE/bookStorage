using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category> AddAsync(Category category, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
