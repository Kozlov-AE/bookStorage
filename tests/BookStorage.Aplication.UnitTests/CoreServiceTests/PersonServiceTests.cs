using AutoFixture;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookStorage.Aplication.UnitTests.CoreServiceTests;

public class PersonServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IPersonRepository> _personRepoMock;
    private readonly PersonService _service;

    public PersonServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<DateOnly>(
            composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));

        _uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _personRepoMock = new Mock<IPersonRepository>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<PersonService>>(MockBehavior.Strict);

        _uowMock.Setup(u => u.Persons).Returns(_personRepoMock.Object);

        // Global logger setup — allow all Log calls
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _service = new PersonService(_uowMock.Object, loggerMock.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPersons()
    {
        // Arrange
        var persons = _fixture.CreateMany<Person>(3).ToList();

        _personRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        _personRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Arrange
        _personRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion GetAllAsync

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsPerson()
    {
        // Arrange
        var personId = Guid.CreateVersion7();
        var person = _fixture.Build<Person>()
            .With(p => p.Id, personId)
            .Create();

        _personRepoMock
            .Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(personId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var personId = Guid.CreateVersion7();

        _personRepoMock
            .Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        Assert.Null(result);
    }

    #endregion GetByIdAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_NewPerson_SavesAndReturns()
    {
        // Arrange
        var person = new Person
        {
            FullName = "New Author",
            Birthday = new DateOnly(1990, 5, 15)
        };
        var savedPerson = new Person
        {
            Id = Guid.CreateVersion7(),
            FullName = "New Author",
            Birthday = new DateOnly(1990, 5, 15)
        };

        _personRepoMock
            .Setup(r => r.GetByName("New Author", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _personRepoMock
            .Setup(r => r.AddAsync(It.Is<Person>(p => p.FullName == "New Author"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPerson);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(person);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(savedPerson.Id, result.Id);
        Assert.Equal("New Author", result.FullName);
        _personRepoMock.Verify(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PersonWithNullBirthday_SavesAndReturns()
    {
        // Arrange
        var person = new Person { FullName = "Nameless", Birthday = null };
        var savedPerson = new Person
        {
            Id = Guid.CreateVersion7(),
            FullName = "Nameless",
            Birthday = null
        };

        _personRepoMock
            .Setup(r => r.GetByName("Nameless", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _personRepoMock
            .Setup(r => r.AddAsync(It.Is<Person>(p => p.FullName == "Nameless"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedPerson);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(person);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nameless", result.FullName);
    }



    [Fact]
    public async Task CreateAsync_DuplicateFullNameAndBirthday_ReturnsExisting()
    {
        // Arrange
        var existing = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .With(p => p.Birthday, new DateOnly(1990, 1, 1))
            .Create();

        _personRepoMock
            .Setup(r => r.GetByName("John Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        var newPerson = new Person
        {
            FullName = "John Doe",
            Birthday = new DateOnly(1990, 1, 1)
        };

        // Act
        var result = await _service.CreateAsync(newPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(existing.Birthday, result.Birthday);
        _personRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }



    [Fact]
    public async Task CreateAsync_DuplicateFullNameBothNoBirthday_ReturnsExisting()
    {
        // Arrange
        var existing = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .With(p => p.Birthday, (DateOnly?)null)
            .Create();

        _personRepoMock
            .Setup(r => r.GetByName("John Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        var newPerson = new Person
        {
            FullName = "John Doe",
            Birthday = null
        };

        // Act
        var result = await _service.CreateAsync(newPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        Assert.Null(result.Birthday);
        _personRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }



    [Fact]
    public async Task CreateAsync_ExistingByNameButDifferentBirthday_ReturnsFirstExisting()
    {
        // Arrange
        var existing = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .With(p => p.Birthday, new DateOnly(1990, 1, 1))
            .Create();

        _personRepoMock
            .Setup(r => r.GetByName("John Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        var newPerson = new Person
        {
            FullName = "John Doe",
            Birthday = new DateOnly(1995, 6, 15)
        };

        // Act
        var result = await _service.CreateAsync(newPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(new DateOnly(1990, 1, 1), result.Birthday);
    }

    [Fact]
    public async Task CreateAsync_ExistingNameNewHasBirthdayExistingHasNot_ReturnsFirstExisting()
    {
        // Arrange
        var existing = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .With(p => p.Birthday, (DateOnly?)null)
            .Create();

        _personRepoMock
            .Setup(r => r.GetByName("John Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        var newPerson = new Person
        {
            FullName = "John Doe",
            Birthday = new DateOnly(1990, 1, 1)
        };

        // Act
        var result = await _service.CreateAsync(newPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        Assert.Null(result.Birthday);
    }

    [Fact]
    public async Task CreateAsync_MultipleExistingSameName_PicksCorrectBirthday()
    {
        var existing1 = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .With(p => p.Birthday, new DateOnly(1990, 1, 1))
            .Create();

        var existing2 = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .With(p => p.Birthday, new DateOnly(1991, 1, 1))
            .Create();

        _personRepoMock
            .Setup(r => r.GetByName("John Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing1, existing2]);

        var newPerson = new Person
        {
            FullName = "John Doe",
            Birthday = new DateOnly(1991, 1, 1)
        };

        // Act
        var result = await _service.CreateAsync(newPerson);

        // Assert — should find by Birthday match
        Assert.NotNull(result);
        Assert.Equal(existing2.Id, result.Id);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ExistingPerson_UpdatesAndReturns()
    {
        // Arrange
        var personId = Guid.CreateVersion7();
        var existing = _fixture.Build<Person>()
            .With(p => p.Id, personId)
            .With(p => p.FullName, "Old Name")
            .With(p => p.Birthday, new DateOnly(1980, 1, 1))
            .Create();

        var updatedData = new Person
        {
            Id = personId,
            FullName = "New Name",
            Birthday = new DateOnly(1990, 5, 15)
        };

        _personRepoMock
            .Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(personId, updatedData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.FullName);
        Assert.Equal(new DateOnly(1990, 5, 15), result.Birthday);
        Assert.Equal(personId, result.Id);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingPerson_ReturnsNull()
    {
        // Arrange
        var personId = Guid.CreateVersion7();

        _personRepoMock
            .Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _service.UpdateAsync(personId, new Person { FullName = "Anyone" });

        // Assert
        Assert.Null(result);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingPerson_ReturnsTrue()
    {
        // Arrange
        var personId = Guid.CreateVersion7();

        _personRepoMock
            .Setup(r => r.DeleteAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(personId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingPerson_ReturnsFalse()
    {
        // Arrange
        var personId = Guid.CreateVersion7();

        _personRepoMock
            .Setup(r => r.DeleteAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(personId);

        // Assert
        Assert.False(result);
    }

    #endregion DeleteAsync

    #region SearchAsync

    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsMatchingPersons()
    {
        // Arrange
        var persons = _fixture.Build<Person>()
            .With(p => p.FullName, "John Doe")
            .CreateMany(2)
            .ToList();

        _personRepoMock
            .Setup(r => r.SearchByName("John", It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        // Act
        var result = await _service.SearchAsync("John");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty(string? query)
    {
        // Act
        var result = await _service.SearchAsync(query!);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        // Arrange
        _personRepoMock
            .Setup(r => r.SearchByName("NonExistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.SearchAsync("NonExistent");

        // Assert
        Assert.Empty(result);
    }

    #endregion SearchAsync

    #region CreateAsync_Batch

    [Fact]
    public async Task CreateAsync_Batch_EmptyList_ReturnsEmpty()
    {
        // Act
        var result = await _service.CreateAsync([]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_Batch_AllNew_SavesAll()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { FullName = "Author A", Birthday = new DateOnly(1980, 1, 1) },
            new Person { FullName = "Author B", Birthday = new DateOnly(1990, 2, 2) }
        };

        _personRepoMock
            .Setup(r => r.GetByNames(
                It.Is<IEnumerable<string>>(names => names.Count() == 2),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _personRepoMock
            .Setup(r => r.AddAsync(It.Is<Person>(p => p.FullName == "Author A"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person p, CancellationToken _) => { p.Id = Guid.CreateVersion7(); return p; });

        _personRepoMock
            .Setup(r => r.AddAsync(It.Is<Person>(p => p.FullName == "Author B"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person p, CancellationToken _) => { p.Id = Guid.CreateVersion7(); return p; });

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _service.CreateAsync(people);

        // Assert
        Assert.Equal(2, result.Count());
        _personRepoMock.Verify(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Batch_AllExisting_ReturnsExisting()
    {
        // Arrange
        var existing = _fixture.Build<Person>()
            .With(p => p.FullName, "Existing Author")
            .With(p => p.Birthday, new DateOnly(1980, 1, 1))
            .Create();

        _personRepoMock
            .Setup(r => r.GetByNames(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var people = new List<Person>
        {
            new Person { FullName = "Existing Author", Birthday = new DateOnly(1980, 1, 1) }
        };

        // Act
        var result = await _service.CreateAsync(people);

        // Assert
        Assert.Single(result);
        Assert.Equal(existing.Id, result.First().Id);
        _personRepoMock.Verify(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Batch_MixedNewAndExisting()
    {
        // Arrange
        var existing = _fixture.Build<Person>()
            .With(p => p.FullName, "Existing Author")
            .With(p => p.Birthday, new DateOnly(1980, 1, 1))
            .Create();

        _personRepoMock
            .Setup(r => r.GetByNames(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        _personRepoMock
            .Setup(r => r.AddAsync(It.Is<Person>(p => p.FullName == "New Author"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person p, CancellationToken _) => { p.Id = Guid.CreateVersion7(); return p; });

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var people = new List<Person>
        {
            new Person { FullName = "Existing Author", Birthday = new DateOnly(1980, 1, 1) },
            new Person { FullName = "New Author", Birthday = new DateOnly(2000, 5, 5) }
        };

        // Act
        var result = await _service.CreateAsync(people);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(existing.Id, result.First().Id);
        Assert.Equal("New Author", result.Last().FullName);
        _personRepoMock.Verify(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion CreateAsync_Batch
}
