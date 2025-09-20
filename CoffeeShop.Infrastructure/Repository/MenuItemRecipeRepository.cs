using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class MenuItemRecipeRepository : BaseRepository<MenuItemRecipe>, IMenuItemRecipeRepository
    {
        public MenuItemRecipeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MenuItemRecipe>> GetByMenuItemIdAsync(int menuItemId)
        {
            return await _dbSet.Where(r => r.MenuItemId == menuItemId).ToListAsync();
        }

        public async Task<IEnumerable<MenuItemRecipe>> GetByIngredientIdAsync(int ingredientId)
        {
            return await _dbSet.Where(r => r.IngredientId == ingredientId).ToListAsync();
        }
    }
}


