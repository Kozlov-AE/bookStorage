using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Genre : IHasId
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
