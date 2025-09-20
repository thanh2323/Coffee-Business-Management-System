using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface ICafeTableRepository : IBaseRepository<CafeTable>
    {
        // CafeTable-specific methods can be added here
        Task<CafeTable?> GetByTableNumberAsync(int tableNumber);
        Task<IEnumerable<CafeTable>> GetAvailableTablesAsync();
        Task<IEnumerable<CafeTable>> GetTablesByBranchIdAsync(int branchId);
    }
}