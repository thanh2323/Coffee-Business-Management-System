using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShop.Infrastructure.Data.Configuration
{
    public class CafeTableConfiguration : IEntityTypeConfiguration<CafeTable>
    {
        public void Configure(EntityTypeBuilder<CafeTable> builder)
        {
            // Unique constraint
            builder.HasIndex(t => new { t.BranchId, t.TableNumber })
                .IsUnique();

            // Relationships
            builder.HasOne(t => t.Branch)
                .WithMany(b => b.CafeTables)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Orders)
                .WithOne(o => o.CafeTable)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
