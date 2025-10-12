using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RegisterOwnerAsync(string username, string email, string password);
        Task LogoutAsync();
      
        Task<User?> GetCurrentUserAsync();
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        bool CanManageBranch(User user, Branch branch);
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }

        public static AuthResult Success(User user)
        {
            return new AuthResult { IsSuccess = true, User = user, Message = "Login successful" };
        }

        public static AuthResult Failed(string message)
        {
            return new AuthResult { IsSuccess = false, Message = message };
        }
    }
}
