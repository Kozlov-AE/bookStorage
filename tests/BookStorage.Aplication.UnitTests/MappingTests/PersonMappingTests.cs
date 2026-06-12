using AutoFixture;
using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using MapsterMapper;
using Xunit;

namespace BookStorage.Aplication.UnitTests.MappingTests
{
    public class PersonMappingTests
    {
        private readonly Mapper _mapper;
        private readonly Fixture _fixture;


        public PersonMappingTests()
        {
            _mapper = MapsterTestHelper.GetMapperForTests();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            // Customization for DateOnly creation
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(date => DateOnly.FromDateTime(date)));
        }

        private Person CreateFullPerson()
        {
            return _fixture.Build<Person>()
                .With(p => p.Birthday, new DateOnly(2000, 1, 1))
                .Create();
        }

        private PersonDto CreateSamplePersonDto(Guid? id = null, string? fullName = null, DateOnly? birthday = null)
        {
            return new PersonDto(
                id?.ToString() ?? Guid.CreateVersion7().ToString(),
                fullName ?? "Test Person",
                birthday ?? new DateOnly(1990, 5, 15)
            );
        }

        [Fact]
        public void Map_Person_To_PersonDto_Should_Ok()
        {
            var srcPerson = CreateFullPerson();

            var personDto = _mapper.Map<Person, PersonDto>(srcPerson);

            Assert.NotNull(personDto);
            Assert.Equal(srcPerson.FullName, personDto.FullName);

            Assert.NotNull(personDto.Id);
            var expectedId = Guid.Parse(personDto.Id);

            Assert.Equal(srcPerson.Birthday, personDto.Birthday);
        }

        [Fact]
        public void Map_Person_To_PersonDto_Persons_Id_empty_Should_Ok()
        {
            var srcPerson = CreateFullPerson();
            srcPerson.Id = Guid.Empty;

            var personDto = _mapper.Map<Person, PersonDto>(srcPerson);

            Assert.NotNull(personDto);
            Assert.Equal(srcPerson.FullName, personDto.FullName);
            Assert.Null(personDto.Id);
            Assert.Equal(srcPerson.Birthday, personDto.Birthday);
        }

[Fact]
        public void Map_PersonDto_To_Person_Should_Ok()
        {
            var id = Guid.CreateVersion7();
            var srcPersonDto = CreateSamplePersonDto(id, "Test Person", new DateOnly(1990, 5, 15));

            // Act
            var targetPerson = _mapper.Map<PersonDto, Person>(srcPersonDto);

            // Assert
            Assert.NotNull(targetPerson);
            Assert.Equal(srcPersonDto.FullName, targetPerson.FullName);
            Assert.Equal(srcPersonDto.Birthday, targetPerson.Birthday);
            Assert.Equal(id, targetPerson.Id);
        }

        [Fact]
        public void Map_PersonDto_To_Person_Null_Id_Should_Ok()
        {
            // Arrange
            var srcPersonDto = new PersonDto(null, "Test Person", new DateOnly(1990, 5, 15));

            // Act
            var targetPerson = _mapper.Map<PersonDto, Person>(srcPersonDto);

            // Assert
            Assert.NotNull(targetPerson);
            Assert.Equal(srcPersonDto.FullName, targetPerson.FullName);
            Assert.Equal(srcPersonDto.Birthday, targetPerson.Birthday);
            Assert.Equal(Guid.Empty, targetPerson.Id);
        }

        [Fact]
        public void Map_Person_With_Null_Birthday_To_PersonDto_Should_Ok()
        {
            // Arrange
            var srcPerson = CreateFullPerson();
            srcPerson.Birthday = null;

            // Act
            var personDto = _mapper.Map<Person, PersonDto>(srcPerson);

            // Assert
            Assert.NotNull(personDto);
            Assert.Equal(srcPerson.FullName, personDto.FullName);
            Assert.Null(personDto.Birthday);
            Assert.NotNull(personDto.Id);
        }

        [Fact]
        public void Map_PersonDto_With_Null_Birthday_To_Person_Should_Ok()
        {
            // Arrange
            var srcPersonDto = new PersonDto(Guid.CreateVersion7().ToString(), "Test Person", null);

            // Act
            var targetPerson = _mapper.Map<PersonDto, Person>(srcPersonDto);

            // Assert
            Assert.NotNull(targetPerson);
            Assert.Equal(srcPersonDto.FullName, targetPerson.FullName);
            Assert.Null(targetPerson.Birthday);
        }

        [Fact]
        public void Map_Person_To_PersonDto_Uses_ConstructUsing_Correctly()
        {
            // Arrange
            var srcPerson = CreateFullPerson();

            // Act
            var personDto = _mapper.Map<Person, PersonDto>(srcPerson);

            // Assert - verify ConstructUsing is used
            Assert.NotNull(personDto);
            Assert.Equal(srcPerson.Id.ToString(), personDto.Id);
            Assert.Equal(srcPerson.FullName, personDto.FullName);
            Assert.Equal(srcPerson.Birthday, personDto.Birthday);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Test User")]
        [InlineData("Very Long Name With Special Characters: Testing various Unicode chars: 文化测试")]
        public void Map_Person_To_PersonDto_Different_Names_Should_Ok(string fullName)
        {
            // Arrange
            var srcPerson = CreateFullPerson();
            srcPerson.FullName = fullName;

            // Act
            var personDto = _mapper.Map<Person, PersonDto>(srcPerson);

            // Assert
            Assert.NotNull(personDto);
            Assert.Equal(fullName, personDto.FullName);
        }

        [Fact]
        public void Map_New_PersonDto_To_Person_Should_Have_Correct_Id_Type()
        {
            // Arrange
            var personId = Guid.CreateVersion7().ToString();
            var srcPersonDto = new PersonDto(personId, "Test Person", new DateOnly(1990, 5, 15));

            // Act
            var targetPerson = _mapper.Map<PersonDto, Person>(srcPersonDto);

            // Assert
            Assert.NotNull(targetPerson);
            Assert.IsType<Guid>(targetPerson.Id);
            Assert.Equal(personId, targetPerson.Id.ToString());
        }

    }
}
