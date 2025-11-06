using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IBranchRepository : IBaseRepository<Branch>
    {
        // ✅ Branch-specific methods
        Task<Branch?> GetByIdAsync(int branchId);
        Task<Branch?> GetByNameAsync(string branchName);
        Task<IEnumerable<Branch>> GetActiveBranchesAsync();

        // ✅ Hai phương thức hỗ trợ (để tương thích code cũ và mới)
        Task<IEnumerable<Branch>> GetByBusinessIdAsync(int businessId);
        Task<IEnumerable<Branch>> GetByBusinessAsync(int businessId);

        Task<bool> ExistsByNameAsync(int businessId, string name);
    }
}
