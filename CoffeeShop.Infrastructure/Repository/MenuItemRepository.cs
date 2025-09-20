using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class MenuItemRepository : BaseRepository<MenuItem>, IMenuItemRepository
    {
        public MenuItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(string category)
        {
            return await _dbSet.Where(m => m.Category == category).ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetAvailableItemsAsync()
        {
            return await _dbSet.Where(m => m.IsAvailable).ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> SearchByNameAsync(string name)
        {
            return await _dbSet.Where(m => m.Name.Contains(name)).ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet.Where(m => m.Price >= minPrice && m.Price <= maxPrice).ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetPopularItemsAsync(int topCount)
        {
            // Placeholder: requires sales data; return top by Id as stub
            return await _dbSet.OrderByDescending(m => m.MenuItemId).Take(topCount).ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetItemsByStatusAsync(bool IsAvailable)
        {
            return await _dbSet.Where(m => m.IsAvailable == IsAvailable).ToListAsync();
        }
    }
}


