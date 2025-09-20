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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Role)
               .HasConversion<int>()
               .IsRequired();

            builder.HasIndex(u => u.Username)
               .IsUnique();
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasOne(u => u.Branch)
               .WithMany(b => b.Users)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.StaffProfile)
                .WithOne(sp => sp.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Orders)
               .WithOne(o => o.User)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
