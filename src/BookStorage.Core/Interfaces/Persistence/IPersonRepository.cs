using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Persistence;

public interface IPersonRepository: IRepository<Person>
{
    Task<IEnumerable<Person>> GetByName(string fullName,CancellationToken cancellationToken = default);
    Task<IEnumerable<Person>> GetByNames(IEnumerable<string> fullNames, CancellationToken cancellationToken = default);
    Task<IEnumerable<Person>> SearchByName(string searchPattern, CancellationToken cancellationToken = default);
}
