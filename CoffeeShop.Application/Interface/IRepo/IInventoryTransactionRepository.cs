using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IInventoryTransactionRepository : IBaseRepository<InventoryTransaction>
    {
        // InventoryTransaction-specific methods can be added here
        Task<IEnumerable<InventoryTransaction>> GetByIngredientIdAsync(int ingredientId);
       
        Task<IEnumerable<InventoryTransaction>> GetByTransactionTypeAsync(TransactionType transactionType);

        Task<decimal> GetTotalQuantityChangeAsync(int ingredientId);


    }
}