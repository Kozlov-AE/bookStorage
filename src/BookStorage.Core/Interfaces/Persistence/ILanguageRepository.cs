using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface ILanguageRepository
{
    Task<IEnumerable<Language>> GetAllAsync(CancellationToken ct = default);
    Task<Language?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Language> AddAsync(Language language, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
