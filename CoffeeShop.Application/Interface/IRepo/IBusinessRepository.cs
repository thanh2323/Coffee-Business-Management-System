using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IBusinessRepository : IBaseRepository<Business>
    {
        // Business-specific methods can be added here
        Task<Business?> GetByBusinessNameAsync(string businessName);
        Task<IEnumerable<Business>> GetActiveBusinessesAsync();
    }
}