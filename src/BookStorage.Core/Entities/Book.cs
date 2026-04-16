using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Book : IHasId
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? ISBN { get; set; }
    public int? Pages { get; set; }
    public int? Year { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Guid? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }
    
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public Guid? LanguageId { get; set; }
    public Language? Language { get; set; }
    
    public Guid? CountryId { get; set; }
    public Country? Country { get; set; }
    
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    
    public ICollection<Person> Authors { get; set; } = new List<Person>();
    
    public ICollection<Person> Translators { get; set; } = new List<Person>();
    
    public ICollection<BookFile> Files { get; set; } = new List<BookFile>(); 
}
