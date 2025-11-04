using CoffeeShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.DTO
{
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new();
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
    }

}
