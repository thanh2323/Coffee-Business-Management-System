using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IMenuItemRecipeRepository : IBaseRepository<MenuItemRecipe>
    {
        // MenuItemRecipe-specific methods can be added here
        Task<IEnumerable<MenuItemRecipe>> GetByMenuItemIdAsync(int menuItemId);
        Task<IEnumerable<MenuItemRecipe>> GetByIngredientIdAsync(int ingredientId);
    }
}