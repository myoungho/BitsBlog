using BitsBlog.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace BitsBlog.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly BitsBlogDbContext _context;

        public DatabaseFacade Database => _context.Database;

        protected DbSet<T> Entities => _context.Set<T>();

        public virtual IQueryable<T> AsTracking() => Entities.AsTracking<T>();

        public virtual IQueryable<T> AsNoTracking() => Entities.AsNoTracking<T>();

        public virtual IQueryable<T> AsQueryable(bool tracking = false) => tracking ? AsTracking() : AsNoTracking();

        public Repository(BitsBlogDbContext ctx)
        {
            _context = ctx;
        }

        public virtual IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, D?>> prop) where D : class =>
            _context.Entry(entry).Reference(prop).Query();

        public virtual async Task LoadReferenceAsync(T entry, params Expression<Func<T, object?>>[] props) =>
            await Task.WhenAll(props.Select(propExp => _context.Entry(entry).Reference(propExp).LoadAsync()));

        public virtual IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, IEnumerable<D>>> prop) where D : class =>
            _context.Entry(entry).Collection(prop).Query();

        public virtual async Task LoadCollectionAsync(T entry, params Expression<Func<T, IEnumerable<object>>>[] props) =>
            await Task.WhenAll(props.Select(propExp => _context.Entry(entry).Collection(propExp).LoadAsync()));

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
            => await Entities.FindAsync(new object[] { id }, ct);

        public Task<T?> GetByIdAsync(int id) => GetByIdAsync(id, default);

        public async Task<T?> GetByKeyAsync(CancellationToken ct = default, params object[] keys)
            => await Entities.FindAsync(keys, ct);

        public async Task<T> InsertAsync(T entity, CancellationToken ct = default)
        {
            await Entities.AddAsync(entity, ct);
            return entity;
        }

        public Task<T> InsertAsync(T entity) => InsertAsync(entity, default);

        public async Task InsertRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => await Entities.AddRangeAsync(entities, ct);

        // Removed GetAllAsync to discourage full table reads

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null ? await AsNoTracking().ToListAsync(ct) : await AsNoTracking().Where(predicate).ToListAsync(ct);

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null ? AsNoTracking().CountAsync(ct) : AsNoTracking().CountAsync(predicate, ct);

        public Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null ? AsNoTracking().AnyAsync(ct) : AsNoTracking().AnyAsync(predicate, ct);

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            Entities.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity) => UpdateAsync(entity, default);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public Task<int> SaveChangesAsync() => SaveChangesAsync(default);

        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            Entities.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity) => DeleteAsync(entity, default);

        public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            Entities.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual EntityState GetEntityState(object entry) => _context.Entry(entry).State;

        public IQueryable<T> Execute(FormattableString query) => Entities.FromSqlInterpolated(query);

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
            => await _context.Database.BeginTransactionAsync(ct);

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await transaction.CommitAsync(ct);
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await transaction.RollbackAsync(ct);
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            await using var tx = await BeginTransactionAsync(ct);
            try
            {
                await action(ct);
                await SaveChangesAsync(ct);
                await CommitTransactionAsync(tx, ct);
            }
            catch
            {
                await RollbackTransactionAsync(tx, ct);
                throw;
            }
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            await using var tx = await BeginTransactionAsync(ct);
            try
            {
                var result = await action(ct);
                await SaveChangesAsync(ct);
                await CommitTransactionAsync(tx, ct);
                return result;
            }
            catch
            {
                await RollbackTransactionAsync(tx, ct);
                throw;
            }
        }


    }
}
