using System.ComponentModel.DataAnnotations;

namespace BookStorage.Infrastructure.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";
    
    [Required] public string BooksPath { get; set; } = "Books";
    public string? MetadataPath { get; set; } = "Metadata";
}
