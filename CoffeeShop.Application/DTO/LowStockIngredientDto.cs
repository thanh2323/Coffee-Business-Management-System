using CoffeeShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.DTO
{

    public class LowStockIngredientDto
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public BaseUnit BaseUnit { get; set; }
    }
}
