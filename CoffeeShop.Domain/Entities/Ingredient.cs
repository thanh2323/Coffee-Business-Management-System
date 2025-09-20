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
    public class Ingredient : BaseEntity
    {
        [Key]
        public int IngredientId { get; set; }

        [ForeignKey(nameof(Branch))]
        public int BranchId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        // Base unit configuration for conversion (e.g., ml, g, pcs)
        [Required]
        public BaseUnit BaseUnit { get; set; }

        // Optional display unit for friendly input (e.g., box_1l, bag_5kg)
        [StringLength(50)]
        public string? DisplayUnit { get; set; }

        // How many base units in one display unit (e.g., 1 box_1l = 1000 ml)
        [Column(TypeName = "decimal(10,3)")]
        public decimal ConversionFactorToBase { get; set; } = 1m;

        // Quantity stored in base units (e.g., ml/g/pcs)
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        // Unit cost currency per base unit

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [NotMapped]
        public decimal StockValue => Quantity * UnitCost;
        // Navigation properties
        public Branch Branch { get; set; } = null!;
        public ICollection<MenuItemRecipe> MenuItemRecipes { get; set; } = new List<MenuItemRecipe>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    }
}
