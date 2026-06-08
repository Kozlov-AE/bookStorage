using System.ComponentModel.DataAnnotations;

namespace BookStorage.Infrastructure.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    [Required] public string Provider { get; set; } = "SQLite";
    [Required] public string ConnectionString { get; set; } = "";
}