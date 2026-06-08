using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookStorage.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IBookRepository? _books;
    private IBookFileRepository? _bookFiles;
    private IPersonRepository? _persons;
    private ICategoryRepository? _categories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IBookRepository Books => _books ??= new BookRepository(_context);
    public IBookFileRepository BookFiles => _bookFiles ??= new BookFileRepository(_context);
    public IPersonRepository Persons => _persons ??= new PersonRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
