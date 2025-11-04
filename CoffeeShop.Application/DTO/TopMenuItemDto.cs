using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.DTO
{

    public class TopMenuItemDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }


}
