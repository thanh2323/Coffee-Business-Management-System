using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IBranchRepository : IBaseRepository<Branch>
    {
        // Branch-specific methods can be added here
        Task<Branch?> GetByNameAsync(string branchName);
        Task<IEnumerable<Branch>> GetActiveBranchesAsync();
    }
}