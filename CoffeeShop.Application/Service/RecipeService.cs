using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Rules;
using Microsoft.AspNetCore.Http;

namespace CoffeeShop.Application.Service;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuthService _authService;
    public RecipeService(IUnitOfWork uow, IAuthService authService)
    {
        _uow = uow;
        _authService = authService;
      
    }

    public async Task<RecipeResult> GetByMenuItemAsync(int branhcId, int menuItemId)
    {

        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RecipeResult.Failed("User not found");

        var branch = await _uow.Branches.GetByIdAsync(branhcId);
        if (branch == null)
            return RecipeResult.Failed("Branch not found");

        var userCanManage = _authService.CanManageBranch(user, branch);
        if (!userCanManage)
            return RecipeResult.Failed("Not authorized to access this menu item");

        var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
        if (menuItem == null)
            return RecipeResult.Failed("Menu item not found");

        // Get recipes
        var recipes = await _uow.MenuItemRecipes.GetByMenuItemIdAsync(menuItemId);
        return RecipeResult.SuccessResult(recipes, "Recipes retrieved successfully");

    }

    public async Task<RecipeResult> AddIngredientAsync(int branchId, int menuItemId, int ingredientId, decimal quantity, string unit)
    {
        try
        {
            // Validate inputs
            if (quantity <= 0)
                return RecipeResult.Failed("Quantity must be greater than 0");


            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return RecipeResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return RecipeResult.Failed("Branch not found");

            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return RecipeResult.Failed("Not authorized to access this menu item");

            var menuItem = await _uow.MenuItems.GetByIdAsync(menuItemId);
            if (menuItem == null)
                return RecipeResult.Failed("Menu item not found");

            var ingredient = await _uow.Ingredients.GetByIdAsync(ingredientId);
            if (ingredient == null)
                return RecipeResult.Failed("Ingredient not found");

            if (ingredient.BranchId != menuItem.BranchId)
                return RecipeResult.Failed("Ingredient must belong to the same branch as menu item");

            if (await _uow.MenuItemRecipes.ExistsAsync(menuItemId, ingredientId))
                return RecipeResult.Failed("This ingredient is already in the recipe");

            // Create new recipe
            var recipe = new MenuItemRecipe
            {
                MenuItemId = menuItemId,
                IngredientId = ingredientId,
                Quantity = quantity,

            };

            _uow.MenuItemRecipes.Add(recipe);
            await _uow.SaveChangesAsync();

            return RecipeResult.SuccessResult(recipe, "Ingredient added to recipe successfully");
        }
        catch (Exception ex)
        {
            return RecipeResult.Failed($"Error adding ingredient to recipe: {ex.Message}");
        }
    }

    public async Task<RecipeResult> UpdateIngredientAsync(int branchId, int menuItemId, int ingredientId, decimal quantity, string unit)
    {



        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RecipeResult.Failed("User not found");

        var branch = await _uow.Branches.GetByIdAsync(branchId);
        if (branch == null)
            return RecipeResult.Failed("Branch not found");

        var userCanManage = _authService.CanManageBranch(user, branch);
        if (!userCanManage)
            return RecipeResult.Failed("Not authorized to access this menu item");

        var menuItem = await _uow.MenuItems.GetByIdAsync(menuItemId);
        if (menuItem == null)
            return RecipeResult.Failed("Menu item not found");

        if (quantity <= 0)
            return RecipeResult.Failed("Quantity must be greater than 0");
        var recipe = await _uow.MenuItemRecipes.GetAsync(menuItemId, ingredientId);
        if (recipe == null)
            return RecipeResult.Failed("Recipe not found");

        // Update recipe
        recipe.Quantity = quantity;


        _uow.MenuItemRecipes.Update(recipe);
        await _uow.SaveChangesAsync();

        return RecipeResult.SuccessResult(recipe, "Recipe updated successfully");
    }



    public async Task<RecipeResult> RemoveIngredientAsync(int branchId, int menuItemId, int ingredientId)
    {

        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RecipeResult.Failed("User not found");

        var branch = await _uow.Branches.GetByIdAsync(branchId);
        if (branch == null)
            return RecipeResult.Failed("Branch not found");

        var userCanManage = _authService.CanManageBranch(user, branch);
        if (!userCanManage)
            return RecipeResult.Failed("Not authorized to access this menu item");

        var menuItem = await _uow.MenuItems.GetByIdAsync(menuItemId);
        if (menuItem == null)
            return RecipeResult.Failed("Menu item not found");




        var recipe = await _uow.MenuItemRecipes.GetAsync(menuItemId, ingredientId);
        if (recipe == null)
            return RecipeResult.Failed("Recipe not found");

        // Remove recipe
        _uow.MenuItemRecipes.SoftDelete(recipe);
        await _uow.SaveChangesAsync();

        return RecipeResult.SuccessResult(recipe, "Ingredient removed from recipe successfully");
    }



    public async Task<RecipeResult> ValidateRecipeAsync(int brachId, int menuItemId)
    {

        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RecipeResult.Failed("User not found");

        var branch = await _uow.Branches.GetByIdAsync(brachId);
        if (branch == null)
            return RecipeResult.Failed("Branch not found");

        var userCanManage = _authService.CanManageBranch(user, branch);
        if (!userCanManage)
            return RecipeResult.Failed("Not authorized to access this menu item");

        var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
        if (menuItem == null)
            return RecipeResult.Failed("Menu item not found");


        var recipes = await _uow.MenuItemRecipes.GetByMenuItemIdAsync(menuItemId);

        // Validate using MenuItemRules
        var canMake = MenuItemRules.CanMake(menuItem);
        if (!canMake)
        {
          // var missingIngredients = MenuItemRules.GetMissingIngredients(recipes);
            return RecipeResult.Failed($"Recipe cannot be made. Missing ingredients:");
        }

        return RecipeResult.SuccessResult(recipes, "Recipe is valid and can be made");


    }


}




