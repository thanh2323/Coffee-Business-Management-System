using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<IEnumerable<Customer>> GetByBranchIdAsync(int branchId)
        {
            return await _dbSet.Where(c => c.BranchId == branchId).ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByLoyaltyTierAsync(LoyaltyTierType tier)
        {
            return await _dbSet.Where(c => c.Tier == tier).ToListAsync();
        }

        // Customer with LoyaltyTransactions (include related data)
        public async Task<Customer?> GetCustomerWithLoyaltyTransactionsAsync(int customerId)
        {
            return await _dbSet
                .Include(c => c.LoyaltyTransactions)
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        // LoyaltyTransaction CRUD methods (since LoyaltyTransaction is part of Customer aggregate)
        public async Task<IEnumerable<LoyaltyTransaction>> GetLoyaltyTransactionsByCustomerIdAsync(int customerId)
        {
            return await _context.LoyaltyTransactions
                .Include(lt => lt.Customer)
                .Include(lt => lt.Order)
                .Where(lt => lt.CustomerId == customerId)
                .OrderByDescending(lt => lt.CreatedAt)
                .ToListAsync();
        }

        public async Task<LoyaltyTransaction?> GetLoyaltyTransactionByIdAsync(int transactionId)
        {
            return await _context.LoyaltyTransactions
                .Include(lt => lt.Customer)
                .Include(lt => lt.Order)
                .FirstOrDefaultAsync(lt => lt.LoyaltyTxnId == transactionId);
        }

        public void AddLoyaltyTransaction(LoyaltyTransaction transaction)
        {
            _context.LoyaltyTransactions.Add(transaction);
        }

        public void DeleteLoyaltyTransaction(LoyaltyTransaction transaction)
        {
            _context.LoyaltyTransactions.Remove(transaction);
        }
    }
}


