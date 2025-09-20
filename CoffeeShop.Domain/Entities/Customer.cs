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
    public class Customer : BaseEntity
    {
        [Key]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(Branch))]
        public int BranchId { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public CustomerType Type { get; set; }

        public int LoyaltyPoints { get; set; } = 0;

        [StringLength(50)]

        public LoyaltyTierType Tier { get; set; } = LoyaltyTierType.Bronze;
        // Navigation properties
        public  Branch Branch { get; set; } = null!;
        public  ICollection<Order> Orders { get; set; } = new List<Order>();
        public  ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();


    }
}
