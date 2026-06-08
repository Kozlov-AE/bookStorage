using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Application;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace BookStorage.Core.Services;

public class PersonService : IPersonService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PersonService> _logger;

    public PersonService(IUnitOfWork unitOfWork, ILogger<PersonService> logger)
    {
        _uow = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Getting all persons");
        var persons = await _uow.Persons.GetAllAsync(ct);
        _logger.LogInformation("Retrieved {PersonsCount} persons", persons.Count());
        return persons;
    }

    public async Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting person with ID: {PersonId}", id);
        var person = await _uow.Persons.GetByIdAsync(id, ct);
        if (person == null)
        {
            _logger.LogWarning("Person with ID {PersonId} not found", id);
        }
        return person;
    }

    public async Task<Person?> CreateAsync(Person person, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating person: {PersonFullName}", person.FullName);
        Person? pers;
        var existingPersons = (await _uow.Persons.GetByName(person.FullName, ct)).ToArray();
        if (existingPersons.Length > 0)
        {
            if (person.Birthday.HasValue)
            {
                var existing = existingPersons.FirstOrDefault(p => p.Birthday == person.Birthday);
                if (existing != null)
                {
                    _logger.LogInformation("Person already exists: {PersonId} - {PersonFullName}", existing.Id, existing.FullName);
                    return existing;
                }
            }
            return existingPersons.FirstOrDefault();
        }
        pers = await CreatePerson(person, ct);
        await _uow.SaveChangesAsync(ct);
        if (pers != null)
        {
            _logger.LogInformation("Person created successfully: {PersonId} - {PersonFullName}", pers.Id, pers.FullName);
        }
        return pers;
    }

    public async Task<IEnumerable<Person>> CreateAsync(IEnumerable<Person> persons, CancellationToken ct = default)
    {
        var personList = persons.ToList();
        _logger.LogInformation("Creating batch of {PersonsCount} persons", personList.Count);
        if (personList.Count == 0)
        {
            _logger.LogDebug("Empty persons list for batch creation");
            return Enumerable.Empty<Person>();
        }

        var existingPersons = (await _uow.Persons.GetByNames(personList.Select(p => p.FullName), ct))
            .ToDictionary(p => p.FullName, StringComparer.OrdinalIgnoreCase);

        var result = new List<Person>();
        foreach (var person in personList)
        {
            if (existingPersons.TryGetValue(person.FullName, out var existing))
            {
                if (person.Birthday.HasValue && existing.Birthday == person.Birthday)
                {
                    result.Add(existing);
                }
                else
                {
                    result.Add(existing);
                }
            }
            else
            {
                await _uow.Persons.AddAsync(person, ct);
                result.Add(person);
            }
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Batch of persons created successfully: {CreatedCount} persons added", result.Count);
        return result;
    }

    public async Task<Person?> UpdateAsync(Guid id, Person person, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating person with ID: {PersonId}", id);
        var existing = await _uow.Persons.GetByIdAsync(id, ct);
        if (existing == null)
        {
            _logger.LogWarning("Person with ID {PersonId} not found for update", id);
            return null;
        }

        existing.Birthday = person.Birthday;
        existing.FullName = person.FullName;
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Person updated successfully: {PersonId} - {PersonFullName}", id, existing.FullName);

        return existing;
    }

    public async Task<IEnumerable<Person>> SearchAsync(string search, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Enumerable.Empty<Person>();

        _logger.LogDebug("Searching persons with pattern: {SearchPattern}", search);
        var persons = (await _uow.Persons.SearchByName(search, ct)).ToList();
        _logger.LogInformation("Found {PersonsCount} persons matching pattern: {SearchPattern}", persons.Count(), search);
        return persons;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting person with ID: {PersonId}", id);
        var result = await _uow.Persons.DeleteAsync(id, ct);
        if (result)
        {
            _logger.LogInformation("Person deleted successfully: {PersonId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to delete person with ID: {PersonId}", id);
        }
        return result;
    }

    private async Task<Person?> CreatePerson(Person person, CancellationToken ct = default)
    {
        var added = await _uow.Persons.AddAsync(person, ct);
        return added;
    }
}