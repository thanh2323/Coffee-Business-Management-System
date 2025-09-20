using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Common;
using CoffeeShop.Domain.Enums;
using System.ComponentModel;

namespace CoffeeShop.Domain.Entities
{
    public class Order : BaseEntity
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CafeTable))]
        public int? TableId { get; set; } // nullable, nếu IsTakeAway = true thì sẽ null

        [ForeignKey(nameof(Branch))]
        public int BranchId { get; set; }

        [ForeignKey(nameof(User))]
        public int? UserId { get; set; }
        [DefaultValue(false)]
        public bool IsTakeAway { get; set; } 

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public DateTime? PaidAt { get; set; }

        public OrderStatus CurrentStatus { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(10,0)")]
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public CafeTable? CafeTable { get; set; }
        public Branch Branch { get; set; } = null!;
        public User? User { get; set; } 
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();


     
    }
}
