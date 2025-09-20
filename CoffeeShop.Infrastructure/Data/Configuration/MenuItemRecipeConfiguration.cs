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
    public class MenuItemRecipeConfiguration : IEntityTypeConfiguration<MenuItemRecipe>
    {
        public void Configure(EntityTypeBuilder<MenuItemRecipe> builder)
        {
            builder.HasIndex(mir => new { mir.MenuItemId, mir.IngredientId })
             .IsUnique();

            // Relationships
            builder.HasOne(mir => mir.MenuItem)
                .WithMany(m => m.MenuItemRecipes)
                .HasForeignKey(mir => mir.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mir => mir.Ingredient)
                .WithMany(i => i.MenuItemRecipes)
                .HasForeignKey(mir => mir.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
