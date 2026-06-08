using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface ICategoryRepository: IRepository<Category>
{
    Task<IEnumerable<Category>> GetByName(string name, CancellationToken cancellationToken = default);
}
