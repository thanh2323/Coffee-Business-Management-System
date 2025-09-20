using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Domain.Rules
{
    public static class DomainRules
    {
      
        public const decimal MIN_PRICE = 1000;
        public const decimal MAX_PRICE = 500000;
        public const int MAX_QUANTITY = 99;
        public const int MIN_STOCK_ALERT = 10;

        public const int LOYALTY_POINTS_RATIO = 1000; // 1 point per 1000 VND

        // Tier thresholds
        public const int SILVER_TIER_POINTS = 2000;
        public const int GOLD_TIER_POINTS = 5000;


    }
}
