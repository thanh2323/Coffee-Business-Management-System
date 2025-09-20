using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IMenuItemRepository : IBaseRepository<MenuItem>
    {
        // MenuItem-specific methods can be added here
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(string category);
        Task<IEnumerable<MenuItem>> GetAvailableItemsAsync();
        Task<IEnumerable<MenuItem>> SearchByNameAsync(string name);
        Task<IEnumerable<MenuItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<MenuItem>> GetPopularItemsAsync(int topCount);
        Task<IEnumerable<MenuItem>> GetItemsByStatusAsync(bool IsAvailable);
    }
}