using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Persistence;

namespace BookStorage.Infrastructure.Persistence.Repositories;

public class BookFileRepository : Repository<BookFile>, IBookFileRepository
{
    public BookFileRepository(AppDbContext context) : base(context)
    {
    }
}