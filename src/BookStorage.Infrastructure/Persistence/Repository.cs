using BookStorage.Core.Interfaces.Models;
using BookStorage.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStorage.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : class, IHasId
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> AddAsync(T entityToAdd, CancellationToken cancellationToken = default)
    {
        entityToAdd.Id = Guid.CreateVersion7();
        await _dbSet.AddAsync(entityToAdd, cancellationToken);
        return entityToAdd;
    }

    public async Task<IEnumerable<T>> AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var entitiesToAdd = entities.Select(x =>
        {
            x.Id = Guid.CreateVersion7();
            return x;
        }).ToList();
        await _dbSet.AddRangeAsync(entitiesToAdd, cancellationToken);
        return entitiesToAdd;
    }

public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }
    
    public virtual async Task Update(T entityToUpdate)
    {
        _dbSet.Attach(entityToUpdate);
        _context.Entry(entityToUpdate).State = EntityState.Modified;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(id, cancellationToken);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        return true;
    }
    
    public virtual void Delete(T entityToDelete)
    {
        if (_context.Entry(entityToDelete).State == EntityState.Detached)
        {
            _dbSet.Attach(entityToDelete);
        }
        _dbSet.Remove(entityToDelete);
    }
}