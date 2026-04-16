namespace BookStorage.Infrastructure.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";
    
    public string BooksPath { get; set; } = "Books";
}
