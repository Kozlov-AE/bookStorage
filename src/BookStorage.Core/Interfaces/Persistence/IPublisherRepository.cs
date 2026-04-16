using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IPublisherRepository
{
    Task<IEnumerable<Publisher>> GetAllAsync(CancellationToken ct = default);
    Task<Publisher?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Publisher> AddAsync(Publisher publisher, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
