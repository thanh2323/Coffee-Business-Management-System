using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IIngredientRepository : IBaseRepository<Ingredient>
    {
        Task<Ingredient?> GetByIdAsync(int id);

        // ✅ Thêm branchId (bắt buộc để tránh lấy nhầm chi nhánh)
        Task<Ingredient?> GetByNameAsync(string name, int branchId);

        Task<IEnumerable<Ingredient>> GetLowStockIngredientsAsync(int threshold);
        Task<IEnumerable<Ingredient>> GetIngredientsByBranchAsync(int branchId);

        // ✅ Giữ nguyên hàm ExistsByNameInBranchAsync có excludeId
        Task<bool> ExistsByNameInBranchAsync(int branchId, string name, int? excludeId = null);

        Task<IEnumerable<InventoryTransaction>> GetInventoryTransactionsByIngredientIdAsync(int ingredientId);
        Task<InventoryTransaction?> GetInventoryTransactionByIdAsync(int transactionId);
        void AddInventoryTransaction(InventoryTransaction transaction);
        void DeleteInventoryTransaction(InventoryTransaction transaction);
    }
}
