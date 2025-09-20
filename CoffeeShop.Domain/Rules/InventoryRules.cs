using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Domain.Rules
{
    public static class InventoryRules
    {
        // 1. Check if ingredient is low stock
        public static bool IsLowStock(Ingredient ingredient)
        {
            return ingredient.Quantity <= DomainRules.MIN_STOCK_ALERT;
        }

        // 2. Check if ingredient is out of stock
        public static bool IsOutOfStock(Ingredient ingredient)
        {
            return ingredient.Quantity <= 0;
        }

        // 3. Validate stock transaction 
        public static bool IsValidStockChange(decimal currentQuantity, decimal changeAmount)
        {
            return (currentQuantity + changeAmount) >= 0;
        }

        // 4. Get low stock ingredients for a branch
        public static IEnumerable<Ingredient> GetLowStockIngredients(IEnumerable<Ingredient> ingredients)
        {
            return ingredients.Where(IsLowStock);
        }

        // 5. Get out of stock ingredients for a branch
        public static IEnumerable<Ingredient> GetOutOfStockIngredients(IEnumerable<Ingredient> ingredients)
        {
            return ingredients.Where(IsOutOfStock);
        }

        // 6. Calculate stock value (if ingredients had unit costs)
        public static decimal CalculateStockValue(IEnumerable<Ingredient> ingredients)
        {
           
            return ingredients.Sum(i => i.StockValue); 
        }

        // 7. Suggest reorder quantity
        public static decimal SuggestReorderQuantity(Ingredient ingredient)
        {
            // Simple rule: reorder to 3x threshold
            var targetQuantity = DomainRules.MIN_STOCK_ALERT * 3;
            return Math.Max(0, targetQuantity - ingredient.Quantity);
        }
    }
}
