using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BitsBlog.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        DatabaseFacade Database { get; }

        EntityState GetEntityState(object entry);

        IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, D>> prop) where D : class;
        Task LoadReferenceAsync(T entry, params Expression<Func<T, object>>[] props);

        IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, IEnumerable<D>>> prop) where D : class;
        Task LoadCollectionAsync(T entry, params Expression<Func<T, IEnumerable<object>>>[] props);

        Task<T> GetByIdAsync(int id);

        Task<IEnumerable<T>> GetAllAsync();

        Task<T> InsertAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        IQueryable<T> AsTracking();

        IQueryable<T> AsNoTracking();

        Task SaveDbContextChangesAsync();

        IQueryable<T> Execute(FormattableString query);
    }
}
