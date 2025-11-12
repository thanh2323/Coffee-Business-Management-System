using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Service
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _uow;

        public InventoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task DeductInventoryFromOrderAsync(Order order)
        {
            if (order?.OrderItems == null || !order.OrderItems.Any())
                return;

            // Dictionary to summarize the quantity to be subtracted for each ingredient
            var ingredientDeductions = new Dictionary<int, (decimal BaseUnitQuantity, string IngredientName, decimal ConversionFactorToBase)>();

           
            foreach (var orderItem in order.OrderItems)
            {
                var menuItem = orderItem.MenuItem;
                if (menuItem == null || menuItem.MenuItemRecipes == null || !menuItem.MenuItemRecipes.Any())
                    continue;

              
                foreach (var recipe in menuItem.MenuItemRecipes)
                {
                    var ingredientId = recipe.IngredientId;
                    var ingredient = recipe.Ingredient;


                    // Calculate the quantity to be subtracted: orderItem.Quantity * recipe.Quantity
                    var quantityToDeduct = orderItem.Quantity * recipe.Quantity;

                    if (ingredientDeductions.ContainsKey(ingredientId))
                    {
                        var existing = ingredientDeductions[ingredientId];
                        ingredientDeductions[ingredientId] = (existing.BaseUnitQuantity + quantityToDeduct, existing.IngredientName, existing.ConversionFactorToBase);
                    }
                    else
                    {
                        ingredientDeductions[ingredientId] = (quantityToDeduct, ingredient.Name, ingredient.ConversionFactorToBase);
                    }
                }
            }

            // Subtract inventory for each ingredient
            var lowStockWarnings = new List<string>();

            foreach (var deduction in ingredientDeductions)
            {
                var ingredientId = deduction.Key;
                var quantityToDeduct = deduction.Value.BaseUnitQuantity;
                var ingredientName = deduction.Value.IngredientName;
                var conversionFactor = deduction.Value.ConversionFactorToBase;

                //Get ingredients from database (retrieve to ensure latest data)
                var ingredient = await _uow.Ingredients.GetByIdAsync(ingredientId);
                if (ingredient == null)
                {
                  
                    lowStockWarnings.Add($"Ingredient '{ingredientName}' (ID: {ingredientId}) not found");
                    continue;
                }

      
                if (ingredient.BranchId != order.BranchId)
                {
                    lowStockWarnings.Add($"Ingredient '{ingredientName}' (ID: {ingredientId}) does not belong to branch {order.BranchId}");
                    continue;
                }

                // Check if ingredients are sufficient (warn if insufficient)
                var remainingQuantity = quantityToDeduct / conversionFactor; ;
                if (ingredient.Quantity - remainingQuantity < 0)
                {
                    lowStockWarnings.Add($"Insufficient stock for '{ingredientName}': Need {remainingQuantity:F2} packages, Have {ingredient.Quantity:F2}");
                }
                // Subtract the amount (can be negative if not enough, for warning)
                ingredient.Quantity -= remainingQuantity;
                ingredient.UpdatedAt = DateTime.UtcNow;

                // Create InventoryTransaction to log history
                var menuItemNames = order.OrderItems
                    .Where(oi => oi.MenuItem?.MenuItemRecipes?.Any(r => r.IngredientId == ingredientId) == true)
                    .Select(oi => oi.MenuItem!.Name)
                    .Distinct()
                    .ToList();

                var transaction = new InventoryTransaction
                {
                    IngredientId = ingredientId,
                    QuantityChange = -quantityToDeduct, // Negative number because it is minus warehouse
                    Type = TransactionType.Export,
                    Notes = $"Auto-deducted from Order #{order.OrderId} - {string.Join(", ", menuItemNames)} - Quantity: {quantityToDeduct}",
                    CreatedAt = DateTime.UtcNow
                };

                _uow.Ingredients.Update(ingredient);
                _uow.Ingredients.AddInventoryTransaction(transaction);
            }

      
        }
    }

}
