using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Book>> GetOverdueBooksAsync(CancellationToken cancellationToken = default);
    Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default);
}

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.BorrowedByReaderId == null)
            .Include(b => b.Author)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Book>> GetOverdueBooksAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(b => b.DueDate.HasValue && b.DueDate.Value < now)
            .Include(b => b.BorrowedByReader)
            .Include(b => b.Author)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.ISBN == isbn, cancellationToken);
    }
}
