using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IBranchRepository : IBaseRepository<Branch>
    {
        // Branch-specific methods can be added here
        Task<Branch?> GetByIdAsync (int branchId);
        Task<Branch?> GetByNameAsync(string branchName);
        Task<IEnumerable<Branch>> GetActiveBranchesAsync();
        Task<IEnumerable<Branch>> GetByBusinessIdAsync(int businessId);
        Task<bool> ExistsByNameAsync(int businessId, string name);
    }
}