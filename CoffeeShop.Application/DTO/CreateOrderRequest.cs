using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.DTO
{
   public class CreateOrderRequest
    {
        public int BranchId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public bool IsTakeAway { get; set; }

      
        public List<CreateOrderItemRequest> OrderItems { get; set; } = new();
    }
}
