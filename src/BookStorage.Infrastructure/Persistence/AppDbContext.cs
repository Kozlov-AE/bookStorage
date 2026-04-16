using BookStorage.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStorage.Infrastructure.Persistence;

public class AppDbContext: DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<BookFile> Files { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
}