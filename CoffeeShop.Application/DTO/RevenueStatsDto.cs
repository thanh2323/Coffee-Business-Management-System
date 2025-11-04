using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.DTO
{
    public class RevenueStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal YesterdayRevenue { get; set; }
        public decimal PercentageChange { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
