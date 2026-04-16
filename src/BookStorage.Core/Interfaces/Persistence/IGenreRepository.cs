using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetAllAsync(CancellationToken ct = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Genre>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<Genre> AddAsync(Genre genre, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
