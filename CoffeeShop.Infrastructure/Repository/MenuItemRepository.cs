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

        // MenuItem with Recipes (include related data)
        public async Task<MenuItem?> GetMenuItemWithRecipesAsync(int menuItemId)
        {
            return await _dbSet
                .Include(m => m.MenuItemRecipes)
                    .ThenInclude(r => r.Ingredient)
                .Include(m => m.Branch)
                .FirstOrDefaultAsync(m => m.MenuItemId == menuItemId);
        }

        // MenuItemRecipe CRUD methods (since MenuItemRecipe is part of MenuItem aggregate)
        public async Task<IEnumerable<MenuItemRecipe>> GetRecipesByMenuItemIdAsync(int menuItemId)
        {
            return await _context.MenuItemRecipes
                .Include(r => r.Ingredient)
                .Include(r => r.MenuItem)
                .Where(r => r.MenuItemId == menuItemId)
                .ToListAsync();
        }

        public async Task<MenuItemRecipe?> GetRecipeByIdAsync(int recipeId)
        {
            return await _context.MenuItemRecipes
                .Include(r => r.Ingredient)
                .Include(r => r.MenuItem)
                .FirstOrDefaultAsync(r => r.Id == recipeId);
        }

        public void AddRecipe(MenuItemRecipe recipe)
        {
            _context.MenuItemRecipes.Add(recipe);
        }

        public void UpdateRecipe(MenuItemRecipe recipe)
        {
            _context.MenuItemRecipes.Update(recipe);
        }

        public void DeleteRecipe(MenuItemRecipe recipe)
        {
            _context.MenuItemRecipes.Remove(recipe);
        }

        public void DeleteRecipesByMenuItemId(int menuItemId)
        {
            var recipes = _context.MenuItemRecipes
                .Where(r => r.MenuItemId == menuItemId)
                .ToList();

            if (recipes.Any())
            {
                _context.MenuItemRecipes.RemoveRange(recipes);
            }
        }
    }
}


