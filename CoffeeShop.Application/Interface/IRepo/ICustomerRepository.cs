using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        // Customer-specific methods
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<IEnumerable<Customer>> GetByBranchIdAsync(int branchId);
        Task<IEnumerable<Customer>> GetByLoyaltyTierAsync(LoyaltyTierType tier);
     

        // Customer with LoyaltyTransactions (include related data)
        Task<Customer?> GetCustomerWithLoyaltyTransactionsAsync(int customerId);
        
        // LoyaltyTransaction CRUD methods (since LoyaltyTransaction is part of Customer aggregate)
        Task<IEnumerable<LoyaltyTransaction>> GetLoyaltyTransactionsByCustomerIdAsync(int customerId);
        Task<LoyaltyTransaction?> GetLoyaltyTransactionByIdAsync(int transactionId);
        void AddLoyaltyTransaction(LoyaltyTransaction transaction);
        void DeleteLoyaltyTransaction(LoyaltyTransaction transaction);
    }
}