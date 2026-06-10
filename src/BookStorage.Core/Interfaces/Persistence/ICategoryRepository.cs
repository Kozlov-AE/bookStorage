using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface ICategoryRepository: IRepository<Category>
{
    Task<IEnumerable<Category>> GetByName(string name, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithChildsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a category with the same name exists in the parent category.
    /// </summary>
    /// <param name="name">Category name</param>
    /// <param name="parentId">Parent category id</param>
    /// <param name="cancellationToken"></param>
    /// <returns>true if category found</returns>
    Task<bool> CheckCategorySameNameAsync(string name, Guid? parentId, CancellationToken cancellationToken = default);
}
