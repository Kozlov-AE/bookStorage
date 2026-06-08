using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStorage.Infrastructure.Persistence.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Book?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Authors)
            .Include(b => b.Files)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}