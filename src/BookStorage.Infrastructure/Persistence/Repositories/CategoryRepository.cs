using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStorage.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetByName(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.Name == name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.ToListAsync(cancellationToken: cancellationToken);
    }
}