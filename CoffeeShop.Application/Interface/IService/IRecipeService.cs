using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService;

public interface IRecipeService
{
    Task<RecipeResult> GetByMenuItemAsync(int branchId, int menuItemId);
    Task<RecipeResult> AddIngredientAsync(int branchId, int menuItemId, int ingredientId, decimal quantity, string unit);
    Task<RecipeResult> UpdateIngredientAsync(int branchId, int menuItemId, int ingredientId, decimal quantity, string unit);
    Task<RecipeResult> RemoveIngredientAsync(int branchId, int menuItemId, int ingredientId);
    Task<RecipeResult> ValidateRecipeAsync(int branchId, int menuItemId);
}

public class RecipeResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<MenuItemRecipe>? Recipes { get; set; }
    public MenuItemRecipe? Recipe { get; set; }

    public static RecipeResult SuccessResult(IEnumerable<MenuItemRecipe> recipes, string message = "Success")
    {
        return new RecipeResult { IsSuccess = true, Message = message, Recipes = recipes };
    }

    public static RecipeResult SuccessResult(MenuItemRecipe recipe, string message = "Success")
    {
        return new RecipeResult { IsSuccess = true, Message = message, Recipe = recipe };
    }

    public static RecipeResult Failed(string message)
    {
        return new RecipeResult { IsSuccess = false, Message = message };
    }
}




