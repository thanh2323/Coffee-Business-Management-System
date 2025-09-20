using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Common;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CoffeeShop.Domain.Entities
{
    public class CafeTable : BaseEntity
    {
        [Key]
        public int TableId { get; set; }

        [ForeignKey(nameof(Branch))]
        public int BranchId { get; set; }

        [Required]
        
        public int TableNumber { get; set; }

        [StringLength(500)]
        public string? QRCode { get; set; }
        [DefaultValue(false)]
        public bool IsActive { get; set; } 

        // Navigation properties
        public  Branch Branch { get; set; } = null!;
        public  ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
