namespace BookStorage.Infrastructure.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    public string Provider { get; set; } = "SQLite";
    public string ConnectionString { get; set; } = "";
}