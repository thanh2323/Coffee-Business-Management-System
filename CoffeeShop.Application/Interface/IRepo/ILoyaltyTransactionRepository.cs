using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface ILoyaltyTransactionRepository : IBaseRepository<LoyaltyTransaction>
    {
        // LoyaltyTransaction-specific methods can be added here
        Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<LoyaltyTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<LoyaltyTransaction>> GetByTransactionTypeAsync(LoyaltyTransactionType transactionType);
  
        Task<decimal> GetCustomerTotalPointsAsync(int customerId);
        Task<decimal> GetCustomerEarnedPointsAsync(int customerId);
        Task<decimal> GetCustomerRedeemedPointsAsync(int customerId);
    }
}