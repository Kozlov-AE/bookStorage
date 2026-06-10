using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IRepository <T> where T : class, IHasId
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    void Update(T entityToUpdate);
    Task<bool> DeleteAsync (Guid id, CancellationToken cancellationToken = default);
    void Delete(T entity);
}