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

        //T GetSingleOrDefault(Expression<Func<T, bool>> where);
        //Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> where);

        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        //IEnumerable<T> GetAllIncludes(params Expression<Func<T, object>>[] includes);
        //Task<IEnumerable<T>> GetAllIncludesAsync(params Expression<Func<T, object>>[] includes);

        //IEnumerable<T> Get(Expression<Func<T, bool>> where);
        //Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> where);

        void Insert(T entity);
        Task<T> InsertAsync(T entity);

        //void Insert(IEnumerable<T> entities);
        //Task InsertAsync(IEnumerable<T> entities);

        void Update(T entity);
        Task UpdateAsync(T entity);

        void Delete(T entity);
        // Task DeleteAsync(T entity);

        //void Delete(Expression<Func<T, bool>> where);
        //Task DeleteAsync(Expression<Func<T, bool>> where);

        //void Delete(IEnumerable<T> entities);
        //Task DeleteAsync(IEnumerable<T> entities);


        //void Replace(IEnumerable<T> removed, IEnumerable<T> added);
        //Task ReplaceAsync(IEnumerable<T> removed, IEnumerable<T> added);

        IQueryable<T> Table { get; }

        IQueryable<T> TableNoTracking { get; }

        void SaveDbContextChanges();

        Task SaveDbContextChangesAsync();

        IQueryable<T> Execute(FormattableString query);

    }
}
