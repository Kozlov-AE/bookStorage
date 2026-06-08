using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Book : IHasId
{
    public Guid Id { get; set; } = Guid.Empty;
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public ICollection<Person> Authors { get; set; } = new List<Person>();
    
    public ICollection<BookFile> Files { get; set; } = new List<BookFile>(); 
}
