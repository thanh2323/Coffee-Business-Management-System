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
    public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
    {
        public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
        {
            // Index for performance
            builder.HasIndex(lt => new { lt.CustomerId, lt.CreatedAt });

            // Relationships
            builder.HasOne(lt => lt.Customer)
                .WithMany(c => c.LoyaltyTransactions)
                .HasForeignKey(lt => lt.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(lt => lt.Order)
                .WithMany(o => o.LoyaltyTransactions)
                .HasForeignKey(lt => lt.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
