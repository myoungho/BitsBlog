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

        public DatabaseFacade Database
        {
            get => _context.Database;
        }

        private DbSet<T> _entities;
        protected DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _context.Set<T>();

                return _entities;
            }
        }


        public virtual IQueryable<T> Table
        {
            get
            {
                return Entities;
            }
        }

        public virtual IQueryable<T> TableNoTracking
        {
            get
            {
                return Entities.AsNoTracking();
            }
        }

        public Repository(BitsBlogDbContext ctx)
        {
            _context = ctx;
        }

        public virtual T GetById(int id)
        {
            return Entities.Find(id);
        }


        public virtual IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, D>> prop) where D : class
        {
            return _context.Entry(entry).Reference(prop).Query();
        }

        public virtual void LoadReference(T entry, params Expression<Func<T, object>>[] props)
        {
            foreach (var propExp in props)
            {
                _context.Entry(entry).Reference(propExp).Load();
            }
        }

        public virtual async Task LoadReferenceAsync(T entry, params Expression<Func<T, object>>[] props)
        {
            List<Task> tasks = new();
            foreach (var propExp in props)
            {
                Task task = _context.Entry(entry).Reference(propExp).LoadAsync();
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        public virtual IQueryable<D> IQueryable<D>(T entry, Expression<Func<T, IEnumerable<D>>> prop) where D : class
        {
            return _context.Entry(entry).Collection(prop).Query();
        }

        public virtual void LoadCollection(T entry, params Expression<Func<T, IEnumerable<object>>>[] props)
        {
            foreach (var propExp in props)
            {
                _context.Entry(entry).Collection(propExp).Load();
            }
        }

        public virtual async Task LoadCollectionAsync(T entry, params Expression<Func<T, IEnumerable<object>>>[] props)
        {
            List<Task> tasks = new();
            foreach (var propExp in props)
            {
                Task task = _context.Entry(entry).Collection(propExp).LoadAsync();
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await Entities.FindAsync(id);
        }

        public void Insert(T entity)
        {
            Entities.Add(entity);
        }

        public async Task<T> InsertAsync(T entity)
        {
            await Entities.AddAsync(entity);
            return entity;
        }

        public IEnumerable<T> GetAll()
        {
            return Entities.ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Entities.ToListAsync();
        }

        public void Update(T entity)
        {
            //Entities.Update(entity);
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public void SaveDbContextChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveDbContextChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Delete(T entity)
        {
            Entities.Remove(entity);
        }

        public virtual EntityState GetEntityState(object entry)
        {
            return _context.Entry(entry).State;
        }

        public IQueryable<T> Execute(FormattableString query)
        {
            return Entities.FromSqlInterpolated(query);
        }



    }
}
