using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BitsBlog.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        DatabaseFacade Database { get; }

        EntityState GetEntityState(object entry);

        // Navigation helpers
        IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, D?>> prop) where D : class;
        Task LoadReferenceAsync(T entry, params Expression<Func<T, object?>>[] props);

        IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, IEnumerable<D>>> prop) where D : class;
        Task LoadCollectionAsync(T entry, params Expression<Func<T, IEnumerable<object>>>[] props);

        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<T?> GetByIdAsync(int id);

        Task<T?> GetByKeyAsync(CancellationToken ct = default, params object[] keys);

        Task<List<TResult>> QueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);

        Task<List<TResult>> ToListAsync<TResult>(IQueryable<TResult> query, CancellationToken cancellationToken = default);
        Task<int> CountAsync<TResult>(IQueryable<TResult> query, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken = default);
        Task<int> CountAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);

        Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

        Task<T> InsertAsync(T entity, CancellationToken ct = default);
        Task<T> InsertAsync(T entity);
        Task InsertRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        IQueryable<T> AsTracking();

        IQueryable<T> AsNoTracking();

        IQueryable<T> AsQueryable(bool tracking = false);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync();

        IQueryable<T> Execute(FormattableString query);

        // Transactions
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default);
        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default);

        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default);
    }
}
