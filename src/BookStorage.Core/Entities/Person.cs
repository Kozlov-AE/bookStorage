using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Person : IHasId
{
    public Guid Id { get; set; } = Guid.Empty;
    public required string FullName { get; set; }
    public DateOnly? Birthday {get; set;}
    public ICollection<Book> AuthoredBooks { get; set; } = new List<Book>();

    public override int GetHashCode()
    {
        return FullName.GetHashCode() ^ (Birthday?.GetHashCode() ?? 0);
    }

    public override bool Equals(object? obj)
    {
        return obj is Person pers
            && pers.FullName == this.FullName
            && pers.Birthday == this.Birthday;
    }
}
