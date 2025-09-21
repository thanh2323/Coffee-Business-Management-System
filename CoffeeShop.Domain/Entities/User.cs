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
    public class User : BaseEntity
    {
        [Key]
        public int UserId { get; set; }

        [ForeignKey(nameof(Business))]
        public int? BusinessId { get; set; }  // null nếu là Admin hệ thống


        [ForeignKey(nameof(Branch))]
        public int? BranchId { get; set; }  // null nếu là Onwer 
        [Required]
        [StringLength(200)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        // Navigation properties
        public Business? Business { get; set; }
        public Branch? Branch { get; set; }
        public StaffProfile? StaffProfile { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
