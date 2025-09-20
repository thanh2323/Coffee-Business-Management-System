using CoffeeShop.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace CoffeeShop.Domain.Entities
{
    public class Branch : BaseEntity
    {
        [Key]
        public int BranchId { get; set; }

        [ForeignKey(nameof(Business))]
        public int BusinessId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        // Navigation properties
        public  Business Business { get; set; } = null!;
        public  ICollection<User> Users { get; set; } = new List<User>();
        public  ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public  ICollection<CafeTable> CafeTables { get; set; } = new List<CafeTable>();
        public  ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public  ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public  ICollection<Order> Orders { get; set; } = new List<Order>();
        public  ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}