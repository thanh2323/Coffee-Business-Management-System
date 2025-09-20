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
    public class InventoryTransaction : BaseEntity
    {
        [Key]
        public int InventoryTxnId { get; set; }

        [ForeignKey(nameof(Ingredient))]
        public int IngredientId { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal QuantityChange { get; set; }

        public TransactionType Type { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public  Ingredient Ingredient { get; set; } = null!;
    }
}
