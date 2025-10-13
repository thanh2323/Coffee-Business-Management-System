using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IMenuItemRepository : IBaseRepository<MenuItem>
    {
        Task<MenuItem?> GetByIdAsync(int id);
        Task<IEnumerable<MenuItem>> GetByBranchIdAsync(int branchId);
        Task<IEnumerable<MenuItem>> GetByBranchAndCategoryAsync(int branchId, string? category);
        Task<MenuItem?> GetByIdWithRecipesAsync(int menuItemId);
        Task<bool> ExistsByNameInBranchAsync(int branchId, string name, int? excludeId = null);
   
    }
}