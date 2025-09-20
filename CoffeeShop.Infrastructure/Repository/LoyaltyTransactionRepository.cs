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
    public class LoyaltyTransactionRepository : BaseRepository<LoyaltyTransaction>, ILoyaltyTransactionRepository
    {
        public LoyaltyTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet.Where(l => l.CustomerId == customerId).ToListAsync();
        }

        public async Task<IEnumerable<LoyaltyTransaction>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate)
        {
            // No date field defined; using CreatedAt from BaseEntity
            return await _dbSet.Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate).ToListAsync();
        }

        public async Task<IEnumerable<LoyaltyTransaction>> GetByTransactionTypeAsync(LoyaltyTransactionType transactionType)
        {
            return await _dbSet.Where(l => l.PointsType == transactionType).ToListAsync();
        }

        public async Task<decimal> GetCustomerTotalPointsAsync(int customerId)
        {
            return await _dbSet
                .Where(l => l.CustomerId == customerId)
                .SumAsync(l => l.PointsType == LoyaltyTransactionType.Earned ? l.Points : -l.Points);
        }

        public async Task<decimal> GetCustomerEarnedPointsAsync(int customerId)
        {
            return await _dbSet
                .Where(l => l.CustomerId == customerId && l.PointsType == LoyaltyTransactionType.Earned)
                .SumAsync(l => l.Points);
        }

        public async Task<decimal> GetCustomerRedeemedPointsAsync(int customerId)
        {
            return await _dbSet
                .Where(l => l.CustomerId == customerId && l.PointsType == LoyaltyTransactionType.Redeemed)
                .SumAsync(l => l.Points);
        }
    }
}


