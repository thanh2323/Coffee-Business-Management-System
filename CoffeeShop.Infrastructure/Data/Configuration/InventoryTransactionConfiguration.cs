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
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            // Index for performance
            builder.HasIndex(it => new { it.IngredientId, it.CreatedAt });
            builder.HasIndex(it => it.Type);

            // Relationships
            builder.HasOne(it => it.Ingredient)
                .WithMany(i => i.InventoryTransactions)
                .HasForeignKey(it => it.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
