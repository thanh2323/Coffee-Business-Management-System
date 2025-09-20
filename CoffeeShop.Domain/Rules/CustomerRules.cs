using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Rules
{
    public static class CustomerRules
    {
        // 1. Determine customer tier based on loyalty points
        public static string GetTier(int loyaltyPoints)
        {
            return loyaltyPoints switch
            {
                >= DomainRules.GOLD_TIER_POINTS => "Gold",
                >= DomainRules.SILVER_TIER_POINTS => "Silver",
                _ => "Bronze"
            };
        }

        // 2. Get discount percentage by tier
        public static decimal GetDiscountPercentage(string tier)
        {
            return tier switch
            {
                "Gold" => 0.10m,    // 10%
                "Silver" => 0.05m,  // 5%
                _ => 0m             // 0%
            };
        }

        // 3. Check if customer can redeem points
        public static bool CanRedeemPoints(Customer customer, int pointsToRedeem)
        {
            return customer.Type == CustomerType.Registered &&
                   customer.LoyaltyPoints >= pointsToRedeem &&
                   pointsToRedeem > 0;
        }

        // 4. Check if customer can earn points
        public static bool CanEarnLoyaltyPoints(Customer customer)
        {
            return customer.Type == CustomerType.Registered;
        }

        // 5. Calculate discount amount for customer
        public static decimal CalculateDiscountAmount(Customer customer, decimal orderTotal)
        {
            if (customer.Type != CustomerType.Registered) return 0;

            var tier = GetTier(customer.LoyaltyPoints);
            var discountPercent = GetDiscountPercentage(tier);
            return orderTotal * discountPercent;
        }
    }
}
