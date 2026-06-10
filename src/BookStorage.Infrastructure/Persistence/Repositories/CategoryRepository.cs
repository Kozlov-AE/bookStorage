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

    public Task<Category?> GetByIdWithChildsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(x=>x.Id==id, cancellationToken: cancellationToken);
    }

    public async Task<bool> CheckCategorySameNameAsync(string name, Guid? parentId, CancellationToken cancellationToken = default)
    {
        var cats = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name && c.ParentCategoryId == parentId, cancellationToken);
        return cats != null;
    }

    public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.ToListAsync(cancellationToken: cancellationToken);
    }
}