using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    // Single Responsibility: Chỉ quản lý User entity
    public interface IUserService
    {
        // Basic CRUD Operations
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user, string password);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);
    //    Task<bool> SoftDeleteAsync(int userId);
    }
}
