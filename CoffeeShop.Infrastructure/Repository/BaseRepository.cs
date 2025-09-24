using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Common;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T entity) => _dbSet.Add(entity);

       
        public void Delete(T entity) => _dbSet.Remove(entity);

        public void SoftDelete(T entity)
        {
            entity.MarkAsDeleted();
            Update(entity);
        }


        public bool Exists(Expression<Func<T, bool>> predicate) => _dbSet.Any(predicate);

        public IEnumerable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public void Update(T entity) => _dbSet.Update(entity);
    }
}
