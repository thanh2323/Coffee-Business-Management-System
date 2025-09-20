using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IBaseRepository<T> where T : class
    {
        // Get all entities
        Task<IEnumerable<T>> GetAllAsync();
        
        // Get entity by ID
        Task<T?> GetByIdAsync(int id);
        
        // Get entities with conditions
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Get single entity with condition
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        
        // Add entity (Unit of Work will handle commit)
        void Add(T entity);
        
        // Add multiple entities (Unit of Work will handle commit)
        void AddRange(IEnumerable<T> entities);
        
        // Update entity (Unit of Work will handle commit)
        void Update(T entity);
        

        
        // Delete entity by entity (Unit of Work will handle commit)
        void Delete(T entity);

        // Check if entity exists
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // Count entities
        Task<int> CountAsync();
        
        // Count entities with condition
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        void SoftDelete(T entity);
        void SoftDeleteRange(IEnumerable<T> entities);
    }
}
