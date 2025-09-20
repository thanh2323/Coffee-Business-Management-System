using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;

namespace CoffeeShop.Application.Interface.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IBusinessRepository Businesses { get; }
        IBranchRepository Branches { get; }
        IUserRepository Users { get; }
        IStaffProfileRepository StaffProfiles { get; }
        ICustomerRepository Customers { get; }
        ICafeTableRepository CafeTables { get; }
        IMenuItemRepository MenuItems { get; }
        IIngredientRepository Ingredients { get; }
        IMenuItemRecipeRepository MenuItemRecipes { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }
        ILoyaltyTransactionRepository LoyaltyTransactions { get; }

        Task<int> SaveChangesAsync();
        int SaveChanges();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
