using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Person : IHasId
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string FirstName { get; set; }
    public required string LastName {get; set;}
    public DateOnly? Birthday {get; set;}
    public ICollection<Book> AuthoredBooks { get; set; } = new List<Book>();
    public ICollection<Book> TranslatedBooks { get; set; } = new List<Book>();
}
