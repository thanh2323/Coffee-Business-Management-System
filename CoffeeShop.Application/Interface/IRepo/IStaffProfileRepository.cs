using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IStaffProfileRepository : IBaseRepository<StaffProfile>
    {
        // StaffProfile-specific methods
        Task<StaffProfile?> GetByUserIdAsync(int userId);
        Task<IEnumerable<StaffProfile>> GetByBranchIdAsync(int branchId);
        Task<IEnumerable<StaffProfile>> GetByRoleAsync(StaffRole role);
        Task<IEnumerable<StaffProfile>> GetActiveStaffAsync();
        Task<IEnumerable<StaffProfile>> GetStaffByBusinessIdAsync(int businessId);
    }
}

