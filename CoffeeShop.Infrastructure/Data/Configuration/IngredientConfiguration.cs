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
    public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
    {
        public void Configure(EntityTypeBuilder<Ingredient> builder)
        {
            builder.HasIndex(i => new { i.BranchId, i.Name });

            builder.HasOne(i => i.Branch)
              .WithMany(b => b.Ingredients)
              .HasForeignKey(i => i.BranchId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(i => i.MenuItemRecipes)
                .WithOne(mir => mir.Ingredient)
                .HasForeignKey(mir => mir.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(i => i.InventoryTransactions)
                .WithOne(it => it.Ingredient)
                .HasForeignKey(it => it.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
