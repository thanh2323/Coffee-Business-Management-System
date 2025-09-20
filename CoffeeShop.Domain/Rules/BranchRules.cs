using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Domain.Rules
{

    public static class BranchRules
    {
        // 1. Check if branch is currently open
        public static bool IsOpen(Branch branch, DateTime? now = null)
        {
            var currentTime = (now ?? DateTime.Now).TimeOfDay;
            return currentTime >= branch.OpenTime && currentTime <= branch.CloseTime;
        }

        // 2. Check if branch can accept new orders
        public static bool IsAcceptingOrders(Branch branch, DateTime? now = null)
        {
            return IsOpen(branch, now);
        }

        // 3. Get business hours display
        public static string GetBusinessHours(Branch branch)
        {
            return $"{branch.OpenTime:hh\\:mm} - {branch.CloseTime:hh\\:mm}";
        }

       
    }
}
