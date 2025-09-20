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
    public class MenuItemRecipe : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(MenuItem))]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(Ingredient))]
        public int IngredientId { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        // Navigation properties
        public  MenuItem MenuItem { get; set; } = null!;
        public  Ingredient Ingredient { get; set; } = null!;
    }
}
