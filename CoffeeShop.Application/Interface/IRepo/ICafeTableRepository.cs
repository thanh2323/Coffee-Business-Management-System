using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface ICafeTableRepository : IBaseRepository<CafeTable>
    {
        Task<CafeTable?> GetByIdAsync(int tableId);
        Task<CafeTable?> GetByBranchAndNumberAsync(int branchId, int tableNumber);
        Task<CafeTable?> GetByQrTokenAsync(string qrToken);
        Task<IEnumerable<CafeTable>> GetByBranchAsync(int branchId);
        Task<IEnumerable<CafeTable>> GetActiveByBranchAsync(int branchId);
    }
}


