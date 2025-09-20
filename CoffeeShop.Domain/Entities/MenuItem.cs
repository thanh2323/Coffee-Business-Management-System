    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Common;
using CoffeeShop.Domain.Rules;

namespace CoffeeShop.Domain.Entities
{
    public class MenuItem : BaseEntity
    {
        [Key]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(Branch))]
        public int BranchId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,0)")]
        public decimal Price { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public  Branch Branch { get; set; } = null!;
        public  ICollection<MenuItemRecipe> MenuItemRecipes { get; set; } = new List<MenuItemRecipe>();
        public  ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();


    
    }
}
