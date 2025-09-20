using System;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoffeeShop.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            
            // Initialize repositories
            Businesses = new BusinessRepository(_context);
            Branches = new BranchRepository(_context);
            Users = new UserRepository(_context);
            StaffProfiles = new StaffProfileRepository(_context);
            Customers = new CustomerRepository(_context);
            MenuItems = new MenuItemRepository(_context);
            Ingredients = new IngredientRepository(_context);
            Orders = new OrderRepository(_context);
        }

        // Repository properties
        public IBusinessRepository Businesses { get; }
        public IBranchRepository Branches { get; }
        public IUserRepository Users { get; }
        public IStaffProfileRepository StaffProfiles { get; }
        public ICustomerRepository Customers { get; }
        public IMenuItemRepository MenuItems { get; }
        public IIngredientRepository Ingredients { get; }
        public IOrderRepository Orders { get; }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        // Transaction management
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Dispose
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
