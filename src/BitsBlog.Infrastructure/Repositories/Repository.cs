using BitsBlog.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BitsBlog.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly BitsBlogDbContext _context;

        public DatabaseFacade Database => _context.Database;

        protected DbSet<T> Entities => _context.Set<T>();

        public virtual IQueryable<T> Table => Entities;

        public virtual IQueryable<T> TableNoTracking => Entities.AsNoTracking();

        public Repository(BitsBlogDbContext ctx)
        {
            _context = ctx;
        }

        public virtual T GetById(int id) => Entities.Find(id);


        public virtual IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, D>> prop) where D : class =>
            _context.Entry(entry).Reference(prop).Query();

        public virtual void LoadReference(T entry, params Expression<Func<T, object>>[] props)
        {
            foreach (var propExp in props)
            {
                _context.Entry(entry).Reference(propExp).Load();
            }
        }

        public virtual async Task LoadReferenceAsync(T entry, params Expression<Func<T, object>>[] props) =>
            await Task.WhenAll(props.Select(propExp => _context.Entry(entry).Reference(propExp).LoadAsync()));

        public virtual IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, IEnumerable<D>>> prop) where D : class =>
            _context.Entry(entry).Collection(prop).Query();

        public virtual void LoadCollection(T entry, params Expression<Func<T, IEnumerable<object>>>[] props)
        {
            foreach (var propExp in props)
            {
                _context.Entry(entry).Collection(propExp).Load();
            }
        }

        public virtual async Task LoadCollectionAsync(T entry, params Expression<Func<T, IEnumerable<object>>>[] props) =>
            await Task.WhenAll(props.Select(propExp => _context.Entry(entry).Collection(propExp).LoadAsync()));

        public async Task<T> GetByIdAsync(int id) => await Entities.FindAsync(id);

        public void Insert(T entity) => Entities.Add(entity);

        public async Task<T> InsertAsync(T entity)
        {
            await Entities.AddAsync(entity);
            return entity;
        }

        public IEnumerable<T> GetAll() => Entities.ToList();

        public async Task<IEnumerable<T>> GetAllAsync() => await Entities.ToListAsync();

        public void Update(T entity) => Entities.Update(entity);

        public Task UpdateAsync(T entity)
        {
            Entities.Update(entity);
            return Task.CompletedTask;
        }

        public void SaveDbContextChanges() => _context.SaveChanges();

        public async Task SaveDbContextChangesAsync() => await _context.SaveChangesAsync();

        public void Delete(T entity) => Entities.Remove(entity);

        public virtual EntityState GetEntityState(object entry) => _context.Entry(entry).State;

        public IQueryable<T> Execute(FormattableString query) => Entities.FromSqlInterpolated(query);



    }
}
