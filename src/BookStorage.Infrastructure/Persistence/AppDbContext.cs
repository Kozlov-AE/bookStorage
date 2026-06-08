using BookStorage.Core.Entities;
using BookStorage.Infrastructure.Configuration;
using BookStorage.Infrastructure.Persistence.EntityTypeConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookStorage.Infrastructure.Persistence;

public class AppDbContext: DbContext
{
    private readonly DatabaseOptions _options;
    
    public DbSet<Book> Books { get; set; }
    public DbSet<BookFile> Files { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Person> Persons { get; set; }

    public AppDbContext(IOptions<DatabaseOptions> dbConfigOptions)
    {
        _options = dbConfigOptions.Value;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_options.Provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(_options.ConnectionString);
        }
        else
        {
            throw new NotSupportedException($"Database provider '{_options.Provider}' is not supported.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new BookConfiguration().Configure(modelBuilder.Entity<Book>());
        new BookFileConfiguration().Configure(modelBuilder.Entity<BookFile>());
        new CategoryConfiguration().Configure(modelBuilder.Entity<Category>());
        new PersonConfiguration().Configure(modelBuilder.Entity<Person>());
    }
}