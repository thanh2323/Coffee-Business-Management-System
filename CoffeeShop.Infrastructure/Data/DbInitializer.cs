using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if admin user already exists
            var adminExists = await context.Users.AnyAsync(u => u.Username == "admin");
            if (!adminExists)
            {
                await CreateAdminUserAsync(context);
            }
        }

        private static async Task CreateAdminUserAsync(ApplicationDbContext context)
        {
            var passwordHasher = new PasswordHasher<User>();

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@coffeeshop.com",
                Role = UserRole.Admin,
                BusinessId = null, // Admin không thuộc business nào
                BranchId = null,   // Admin không thuộc branch nào
                CreatedAt = DateTime.UtcNow
            };

            // Hash password
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin123");

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        public static async Task SeedSampleDataAsync(ApplicationDbContext context)
        {
            // Check if sample business exists
            var sampleBusinessExists = await context.Businesses.AnyAsync(b => b.Name == "Sample Coffee Shop");
            if (!sampleBusinessExists)
            {
                await CreateSampleBusinessAsync(context);
            }
        }

        private static async Task CreateSampleBusinessAsync(ApplicationDbContext context)
        {
            var passwordHasher = new PasswordHasher<User>();

            // Create sample business
            var business = new Business
            {
                Name = "Sample Coffee Shop",
                Address = "123 Main Street, Ho Chi Minh City",
                Phone = "0123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Businesses.Add(business);
            await context.SaveChangesAsync();

            // Create sample owner
            var owner = new User
            {
                Username = "owner",
                Email = "owner@samplecoffee.com",
                Role = UserRole.Owner,
                BusinessId = business.BusinessId,
                BranchId = null,
                CreatedAt = DateTime.UtcNow
            };

            owner.PasswordHash = passwordHasher.HashPassword(owner, "owner123");
            context.Users.Add(owner);

            // Create sample branch
            var branch = new Branch
            {
                Name = "Main Branch",
                Address = "123 Main Street, Ho Chi Minh City",
                OpenTime = new TimeSpan(7, 0, 0), // 7:00 AM
                CloseTime = new TimeSpan(22, 0, 0), // 10:00 PM
                BusinessId = business.BusinessId,
                CreatedAt = DateTime.UtcNow
            };

            context.Branches.Add(branch);
            await context.SaveChangesAsync();

            // Create sample staff
            var staff = new User
            {
                Username = "staff",
                Email = "staff@samplecoffee.com",
                Role = UserRole.Staff,
                BusinessId = business.BusinessId,
                BranchId = branch.BranchId,
                CreatedAt = DateTime.UtcNow
            };

            staff.PasswordHash = passwordHasher.HashPassword(staff, "staff123");
            context.Users.Add(staff);

            // Create staff profile
            var staffProfile = new StaffProfile
            {
                UserId = staff.UserId,
                Position = StaffRole.Barista,
                ShiftInfo = "Morning Shift (7:00 - 15:00)",
                SalaryType = "Monthly",
                Salary = 8000000, // 8M VND
                CreatedAt = DateTime.UtcNow
            };

            context.StaffProfiles.Add(staffProfile);
            await context.SaveChangesAsync();
        }
    }
}
