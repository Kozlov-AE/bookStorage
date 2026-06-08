using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class BookFile : IHasId
{
    public Guid Id { get; set; } = Guid.Empty;
    public required string FileName { get; set; }
    public string? FullFilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public string? FileType { get; set; }
    public DateTime UploadedAt { get; set; }
    
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
