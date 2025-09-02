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
        void LoadReference(T entry, params Expression<Func<T, object>>[] props);
        Task LoadReferenceAsync(T entry, params Expression<Func<T, object>>[] props);

        IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, IEnumerable<D>>> prop) where D : class;
        void LoadCollection(T entry, params Expression<Func<T, IEnumerable<object>>>[] props);
        Task LoadCollectionAsync(T entry, params Expression<Func<T, IEnumerable<object>>>[] props);

        T GetById(int id);
        Task<T> GetByIdAsync(int id);

        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        void Insert(T entity);
        Task<T> InsertAsync(T entity);

        void Update(T entity);
        Task UpdateAsync(T entity);

        void Delete(T entity);

        IQueryable<T> Table { get; }

        IQueryable<T> TableNoTracking { get; }

        void SaveDbContextChanges();

        Task SaveDbContextChangesAsync();

        IQueryable<T> Execute(FormattableString query);
    }
}
