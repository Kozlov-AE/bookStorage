using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Application;

public interface IPersonService
{
    Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default);
    Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Person?> CreateAsync(Person person, CancellationToken ct = default);
    Task<IEnumerable<Person>> CreateAsync(IEnumerable<Person> persons, CancellationToken ct = default);
    Task<Person?> UpdateAsync(Guid id, Person person, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Person>> SearchAsync(string search, CancellationToken ct = default);
}