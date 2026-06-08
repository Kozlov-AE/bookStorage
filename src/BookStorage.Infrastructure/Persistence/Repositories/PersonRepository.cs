using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStorage.Infrastructure.Persistence.Repositories;

public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Person>> GetByName(string fullName, CancellationToken cancellationToken)
    {
        var persons = _dbSet.Where(p => (
            p.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase)));
        return await persons.ToArrayAsync(cancellationToken);
    }

    public async Task<IEnumerable<Person>> GetByNames(IEnumerable<string> fullNames, CancellationToken cancellationToken = default)
    {
        var names = fullNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var persons = _dbSet.Where(p => names.Contains(p.FullName));
        return await persons.ToArrayAsync(cancellationToken);
    }

    public async Task<IEnumerable<Person>> SearchByName(string searchPattern, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(p => p.FullName.Contains(searchPattern))
            .ToArrayAsync(cancellationToken);
    }
}