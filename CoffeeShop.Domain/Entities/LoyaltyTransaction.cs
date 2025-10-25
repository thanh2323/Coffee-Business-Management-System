using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Common;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Entities
{
    public class LoyaltyTransaction : BaseEntity
    {
        [Key]
        public int LoyaltyTxnId { get; set; }

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(Order))]
        public int? OrderId { get; set; }

        public LoyaltyTransactionType PointsType { get; set; }
        [Column(TypeName = "decimal(10,0)")]
        public decimal Points { get; set; }


        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        public  Customer Customer { get; set; } = null!;
        public  Order? Order { get; set; }
    }
}
