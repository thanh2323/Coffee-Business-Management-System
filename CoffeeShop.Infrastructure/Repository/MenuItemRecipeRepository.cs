using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository;

public class MenuItemRecipeRepository : BaseRepository<MenuItemRecipe>, IMenuItemRecipeRepository
{
    public MenuItemRecipeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MenuItemRecipe>> GetByMenuItemIdAsync(int menuItemId)
    {
        return await _dbSet.Include(mr => mr.Ingredient)
                           .Where(mr => mr.MenuItemId == menuItemId)
                           .ToListAsync();

    }

    public async Task<MenuItemRecipe?> GetAsync(int menuItemId, int ingredientId)
    {
        return await _dbSet.Include(mr => mr.Ingredient)
                           .FirstOrDefaultAsync(mr => mr.MenuItemId == menuItemId && mr.IngredientId == ingredientId);

    }

    public async Task<bool> ExistsAsync(int menuItemId, int ingredientId)
    {
        return await _dbSet.AnyAsync(mr => mr.MenuItemId == menuItemId && mr.IngredientId == ingredientId);
            
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsByIngredientIdAsync(int ingredientId)
    {
        return await _dbSet.Include(mr => mr.MenuItem)
                           .Where(mr => mr.IngredientId == ingredientId)
                           .Select(mr => mr.MenuItem)
                           .ToListAsync();

    }

    public async Task<IEnumerable<MenuItemRecipe>> GetByIngredientIdAsync(int ingredientId)
    {
        return await _dbSet.Include(mr => mr.MenuItem)
                           .Where(mr => mr.IngredientId == ingredientId)
                           .ToListAsync();

    }
}

