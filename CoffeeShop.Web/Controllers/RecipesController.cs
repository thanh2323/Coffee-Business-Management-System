using CoffeeShop.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers;

[Authorize(Policy = "RequireOwnerOrManager")]
public class RecipesController : Controller
{
    private readonly IRecipeService _recipeService;
    private readonly IMenuItemService _menuItemService;
    private readonly IIngredientService _ingredientService;

    public RecipesController(
        IRecipeService recipeService,
        IMenuItemService menuItemService,
        IIngredientService ingredientService)
    {
        _recipeService = recipeService;
        _menuItemService = menuItemService;
        _ingredientService = ingredientService;
    }

    [HttpGet] 
    public async Task<IActionResult> Index(int branchId, int menuItemId)
    {

        var result = await _recipeService.GetByMenuItemAsync(branchId, menuItemId);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index), new { branchId, menuItemId });
        }

        // Get menu item details for display
        var menuItemResult = await _menuItemService.GetByIdAsync(menuItemId);
        if (!menuItemResult.IsSuccess)
        {
            TempData["Error"] = menuItemResult.Message;
            return RedirectToAction(nameof(Index), new { branchId, menuItemId });
        }
        ViewBag.BranchId = branchId;
        ViewBag.MenuItem = menuItemResult.MenuItem;
        ViewBag.MenuItemId = menuItemId;
        ViewBag.Recipes = result.Recipes ?? Enumerable.Empty<Domain.Entities.MenuItemRecipe>();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> AddIngredient(int branchId, int menuItemId)
    {

        // Get menu item details
        var menuItemResult = await _menuItemService.GetByIdAsync(menuItemId);
        if (!menuItemResult.IsSuccess)
        {
            TempData["Error"] = menuItemResult.Message;
            return RedirectToAction(nameof(Index), new { branchId, menuItemId });
        }

        // Get available ingredients for this branch
        var ingredientsResult = await _ingredientService.GetByBranchAsync(branchId);
    
        ViewBag.MenuItem = menuItemResult.MenuItem;
        ViewBag.MenuItemId = menuItemId;
        ViewBag.AvailableIngredients = ingredientsResult ?? Enumerable.Empty<Domain.Entities.Ingredient>();
        ViewBag.BranchId = branchId; 

        return View();

     
    }

 
    [HttpPost]
    public async Task<IActionResult> AddIngredient(int branchId, int menuItemId, int ingredientId, decimal quantity, string unit)
    {

        var result = await _recipeService.AddIngredientAsync(branchId, menuItemId, ingredientId, quantity, unit);

        if (result.IsSuccess)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { branchId, menuItemId });
    }

    [HttpGet]
    public async Task<IActionResult> EditIngredient(int branchId,int menuItemId, int ingredientId)
    {
      
        // Get current recipe
        var result = await _recipeService.GetByMenuItemAsync(branchId, menuItemId);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Index", new {branchId, menuItemId });
        }

        var recipe = result.Recipes?.FirstOrDefault(r => r.IngredientId == ingredientId);
        if (recipe == null)
        {
            TempData["Error"] = "Recipe not found";
            return RedirectToAction("Index", new {branchId ,menuItemId });
        }
        ViewBag.BranchId = branchId;
        ViewBag.MenuItemId = menuItemId;
        ViewBag.IngredientId = ingredientId;
        ViewBag.Recipe = recipe;

        return View();
    }

   
    [HttpPost]
    public async Task<IActionResult> EditIngredient(int branchId, int menuItemId, int ingredientId, decimal quantity, string unit)
    {

        var result = await _recipeService.UpdateIngredientAsync(branchId, menuItemId, ingredientId, quantity, unit);

        if (result.IsSuccess)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction("Index", new {branchId ,menuItemId });
    }


    [HttpPost]
    public async Task<IActionResult> RemoveIngredient(int branchId, int menuItemId, int ingredientId)
    {
       

        var result = await _recipeService.RemoveIngredientAsync(branchId, menuItemId, ingredientId);

        if (result.IsSuccess)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction("Index", new {branchId, menuItemId });
    }

    [HttpPost]
    public async Task<IActionResult> Validate(int branchId, int menuItemId)
    {
        

        var result = await _recipeService.ValidateRecipeAsync(branchId, menuItemId);

        if (result.IsSuccess)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction("Index", new {branchId ,menuItemId });
    }


}




