using CoffeeShop.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Entities
{
    public class StaffProfile : BaseEntity
    {
        [Key]
        public int StaffId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public StaffRole Position { get; set; }
        [StringLength(200)]
        public string? ShiftInfo { get; set; }

        [StringLength(20)]
        public string? SalaryType { get; set; }

        [Column(TypeName = "decimal(10,0)")]
        public decimal? Salary { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}