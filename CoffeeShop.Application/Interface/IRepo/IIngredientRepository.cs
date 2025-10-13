using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IIngredientRepository : IBaseRepository<Ingredient>
    {
        // Ingredient-specific methods
        Task<Ingredient?> GetByIdAsync(int id);
        Task<Ingredient?> GetByNameAsync(string name);
        Task<IEnumerable<Ingredient>> GetLowStockIngredientsAsync(int threshold);
        Task<IEnumerable<Ingredient>> GetIngredientsByBranchAsync(int branchId);

        Task<bool> ExistsByNameInBranchAsync(int branchId, string name, int? excludeId = null);
        // InventoryTransaction CRUD methods (since InventoryTransaction is part of Ingredient aggregate)
        Task<IEnumerable<InventoryTransaction>> GetInventoryTransactionsByIngredientIdAsync(int ingredientId);
        Task<InventoryTransaction?> GetInventoryTransactionByIdAsync(int transactionId);
        void AddInventoryTransaction(InventoryTransaction transaction);
        void DeleteInventoryTransaction(InventoryTransaction transaction);
    }
}