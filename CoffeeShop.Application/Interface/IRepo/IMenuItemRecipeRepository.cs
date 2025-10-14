using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo;

public interface IMenuItemRecipeRepository : IBaseRepository<MenuItemRecipe>
{
    Task<IEnumerable<MenuItemRecipe>> GetByMenuItemIdAsync(int menuItemId);
    Task<MenuItemRecipe?> GetAsync(int menuItemId, int ingredientId);
    Task<bool> ExistsAsync(int menuItemId, int ingredientId);
    Task<IEnumerable<MenuItem>> GetMenuItemsByIngredientIdAsync(int ingredientId);
    Task<IEnumerable<MenuItemRecipe>> GetByIngredientIdAsync(int ingredientId);
}




