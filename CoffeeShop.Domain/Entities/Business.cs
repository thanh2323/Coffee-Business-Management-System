using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Common;

namespace CoffeeShop.Domain.Entities
{
    public class Business : BaseEntity
    {
        [Key]
        public int BusinessId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }
        [DefaultValue(false)]
        public bool IsActive { get; set; }

        // Navigation properties
        public  ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public  ICollection<User> Users { get; set; } = new List<User>();
    }
}
