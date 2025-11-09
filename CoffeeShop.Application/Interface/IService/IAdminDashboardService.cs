using CoffeeShop.Application.DTO;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IAdminDashboardService
    {
        // Get overall system statistics
        Task<AdminSystemStatsDto> GetSystemStatsAsync(DateTime startDate, DateTime endDate);
        
        // Get business statistics
        Task<AdminBusinessStatsDto> GetBusinessStatsAsync();
        
        // Get top performing businesses by revenue
        Task<IEnumerable<TopBusinessDto>> GetTopBusinessesAsync(int limit, DateTime startDate, DateTime endDate);
        
        // Get businesses with expiring subscriptions
        Task<IEnumerable<ExpiringBusinessDto>> GetExpiringBusinessesAsync(int daysAhead = 30);
        
        // Get recent businesses
        Task<IEnumerable<Business>> GetRecentBusinessesAsync(int limit);
        
        // Get revenue time series for all businesses
        Task<IEnumerable<RevenueTimeSeriesDto>> GetSystemRevenueTimeSeriesAsync(DateTime startDate, DateTime endDate);
        
        // Get orders count by status for all businesses
        Task<Dictionary<OrderStatus, int>> GetSystemOrdersByStatusAsync(DateTime startDate, DateTime endDate);
    }

    // DTOs
    public class AdminSystemStatsDto
    {
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int InactiveBusinesses { get; set; }
        public int TotalBranches { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public int TotalUsers { get; set; }
        public int ExpiringSubscriptions { get; set; }
    }

    public class AdminBusinessStatsDto
    {
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int InactiveBusinesses { get; set; }
        public int NewBusinessesThisMonth { get; set; }
        public int BusinessesWithExpiringSubscription { get; set; }
    }

    public class TopBusinessDto
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int BranchCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class ExpiringBusinessDto
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public DateTime? SubscriptionEndDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsActive { get; set; }
    }
}
