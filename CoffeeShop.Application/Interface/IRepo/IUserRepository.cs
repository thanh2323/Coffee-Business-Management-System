using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IUserRepository : IBaseRepository<User>
    {
        // User-specific methods
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetStaffByBranchAsync(int branchId);

        // 🟢 Thêm dòng này để Owner xem tất cả nhân viên trong business
        Task<IEnumerable<User>> GetStaffByBusinessAsync(int businessId);
    }
}
