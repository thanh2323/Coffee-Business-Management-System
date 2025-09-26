using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Infrastructure.UnitOfWork;

namespace CoffeeShop.Application.Interface.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IBusinessRepository Businesses { get; }
        IBranchRepository Branches { get; }
        IUserRepository Users { get; }
        IStaffProfileRepository StaffProfiles { get; }
        ICustomerRepository Customers { get; }
        IMenuItemRepository MenuItems { get; }
        IIngredientRepository Ingredients { get; }
        IOrderRepository Orders { get; }
        ICafeTableRepository CafeTables { get; }

        Task<int> SaveChangesAsync();
        int SaveChanges();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
