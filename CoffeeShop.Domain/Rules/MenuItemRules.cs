using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Domain.Rules
{
    public static class MenuItemRules
    {
        // 1. Validate menu item price
        public static bool IsValidPrice(decimal price)
        {
            return price >= DomainRules.MIN_PRICE &&
                   price <= DomainRules.MAX_PRICE;
        }

        // 2. Check if menu item can be made with current ingredients
        public static bool CanMake(MenuItem menuItem)
        {
            return CanMakeQuantity(menuItem, 1);
        }

        // 3. Check if specific quantity can be made
        public static bool CanMakeQuantity(MenuItem menuItem, int quantity)
        {
            foreach (var recipe in menuItem.MenuItemRecipes)
            {
                var required = recipe.Quantity * quantity;
                if (recipe.Ingredient.Quantity < required)
                    return false;
            }
            return true;
        }

        // 4. Get required ingredients for menu item
        public static Dictionary<string, decimal> GetRequiredIngredients(MenuItem menuItem, int quantity = 1)
        {
            var requirements = new Dictionary<string, decimal>();

            foreach (var recipe in menuItem.MenuItemRecipes)
            {
                var required = recipe.Quantity * quantity;
                requirements[recipe.Ingredient.Name] = required;
            }

            return requirements;
        }

        // 5. Check if menu item should be available
        public static bool ShouldBeAvailable(MenuItem menuItem)
        {
            return CanMake(menuItem);
        }
        // 6. Calculate ingredient cost for menu item (COGS)
        public static decimal CalculateIngredientCost(MenuItem menuItem, int quantity = 1)
        {
            decimal totalCost = 0;

            foreach (var recipe in menuItem.MenuItemRecipes)
            {
                var requiredQty = recipe.Quantity * quantity;
                totalCost += requiredQty * recipe.Ingredient.UnitCost;
            }

            return totalCost;
        }

        // 7. Validate if selling price is reasonable vs cost
        public static bool IsSellingPriceValid(MenuItem menuItem)
        {
            var cost = CalculateIngredientCost(menuItem);
            // Rule: bán ra phải >= 120% giá vốn
            return menuItem.Price >= cost * 1.2m;
        }
    }
}