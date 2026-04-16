using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IPersonRepository
{
    Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default);
    Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Person>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<Person> AddAsync(Person person, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
