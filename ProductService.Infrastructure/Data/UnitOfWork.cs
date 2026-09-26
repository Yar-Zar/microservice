namespace ProductService.Infrastructure.Data;
public class UnitOfWork : IUnitOfWork
{
    private readonly ProductDbContext _db;
    public UnitOfWork(ProductDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Database.BeginTransactionAsync(cancellationToken);
    }
    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

