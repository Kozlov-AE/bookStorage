using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface ICountryRepository
{
    Task<IEnumerable<Country>> GetAllAsync(CancellationToken ct = default);
    Task<Country?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Country> AddAsync(Country country, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
