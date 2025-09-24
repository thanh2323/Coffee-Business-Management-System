using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IBaseRepository<T> where T : class
    {
   
        Task<IEnumerable<T>> GetAllAsync();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void SoftDelete(T entity);
        bool Exists(Expression<Func<T, bool>> predicate);
    }
}
