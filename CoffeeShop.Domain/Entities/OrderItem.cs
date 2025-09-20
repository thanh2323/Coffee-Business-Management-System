using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Common;

namespace CoffeeShop.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        [Key]
        public int OrderItemId { get; set; }

        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }

        [ForeignKey(nameof(MenuItem))]
        public int MenuItemId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,0)")]
        public decimal Price { get; set; }

        // Navigation properties
        public  Order Order { get; set; } = null!;
        public  MenuItem MenuItem { get; set; } = null!;
    }
}
