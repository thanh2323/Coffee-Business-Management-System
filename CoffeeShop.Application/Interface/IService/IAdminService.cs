using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IAdminService
    {
        // Activation
        Task<AdminResult> ActivateBusinessAsync(int businessId);
        Task<AdminResult> DeactivateBusinessAsync(int businessId);
        
        // Business management
        Task<IEnumerable<Business>> GetAllBusinessesAsync();
        Task<Business?> GetBusinessByIdAsync(int businessId);
        Task<IEnumerable<Business>> GetActiveBusinessesAsync();
        Task<IEnumerable<Business>> GetInactiveBusinessesAsync();
        
        // Subscription management
        Task<AdminResult> UpdateSubscriptionAsync(int businessId, DateTime endDate, decimal monthlyFee);
        Task<AdminResult> CheckExpiredSubscriptionsAsync();
    }

    public class AdminResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Business? Business { get; set; }

        public static AdminResult Success(Business? business, string message = "Operation successful")
        {
            return new AdminResult { IsSuccess = true, Business = business, Message = message };
        }

        public static AdminResult Failed(string message)
        {
            return new AdminResult { IsSuccess = false, Message = message };
        }
    }
}
