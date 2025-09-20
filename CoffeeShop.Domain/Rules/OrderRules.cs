using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Rules
{
    public static class OrderRules
    {
        // 1. Validate order status transition
        public static bool CanChangeStatus(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
                (OrderStatus.Preparing, OrderStatus.Ready) => true,
                (OrderStatus.Ready, OrderStatus.Completed) => true,
                (_, OrderStatus.Cancelled) => currentStatus != OrderStatus.Completed,
                _ => false
            };
        }
        // 2. Calculate order total
        public static decimal CalculateTotal(IEnumerable<OrderItem> orderItems)
        {
            return orderItems.Sum(item => item.Price * item.Quantity);
        }

        // 3. Check if order has sufficient ingredients
        public static bool HasSufficientIngredients(Order order)
        {
            foreach (var orderItem in order.OrderItems)
            {
                if (!MenuItemRules.CanMakeQuantity(orderItem.MenuItem, orderItem.Quantity))
                {
                    return false;
                }
            }
            return true;
        }

        // 4. Get missing ingredients for order
        public static List<string> GetMissingIngredients(Order order)
        {
            var missingIngredients = new List<string>();

            foreach (var orderItem in order.OrderItems)
            {
                foreach (var recipe in orderItem.MenuItem.MenuItemRecipes)
                {
                    var required = recipe.Quantity * orderItem.Quantity;
                    if (recipe.Ingredient.Quantity < required)
                    {
                        var missing = $"{recipe.Ingredient.Name}: need {required}, have {recipe.Ingredient.Quantity}";
                        missingIngredients.Add(missing);
                    }
                }
            }

            return missingIngredients;
        }
    }
}
