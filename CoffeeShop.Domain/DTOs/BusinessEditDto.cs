using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Domain.DTOs
{
    public class BusinessEditDto
    {
        public int BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal MonthlyFee { get; set; }
    }
}
