using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IMenuItemRepository : IBaseRepository<MenuItem>
    {
        // MenuItem-specific methods
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(string category);
        Task<IEnumerable<MenuItem>> GetAvailableItemsAsync();
        Task<IEnumerable<MenuItem>> SearchByNameAsync(string name);
        Task<IEnumerable<MenuItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<MenuItem>> GetPopularItemsAsync(int topCount);
        Task<IEnumerable<MenuItem>> GetItemsByStatusAsync(bool IsAvailable);
        
        // MenuItem with Recipes (include related data)
        Task<MenuItem?> GetMenuItemWithRecipesAsync(int menuItemId);
        
        // MenuItemRecipe CRUD methods (since MenuItemRecipe is part of MenuItem aggregate)
        Task<IEnumerable<MenuItemRecipe>> GetRecipesByMenuItemIdAsync(int menuItemId);
        Task<MenuItemRecipe?> GetRecipeByIdAsync(int recipeId);
        void AddRecipe(MenuItemRecipe recipe);
        void UpdateRecipe(MenuItemRecipe recipe);
        void DeleteRecipe(MenuItemRecipe recipe);
        void DeleteRecipesByMenuItemId(int menuItemId);
    }
}