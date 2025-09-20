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
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {

            builder.Property(c => c.Type)
                .HasConversion<int>()
                .IsRequired();

            // Index for performance
            builder.HasIndex(c => c.Phone)
                    .IsUnique();
            builder.HasIndex(c => c.Type);

            // Relationships
            builder.HasOne(c => c.Branch)
                .WithMany(b => b.Customers)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.LoyaltyTransactions)
                .WithOne(lt => lt.Customer)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
