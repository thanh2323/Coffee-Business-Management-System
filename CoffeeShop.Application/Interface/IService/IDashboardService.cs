using CoffeeShop.Application.DTO;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IDashboardService
    {
        // Get revenue statistics for a business
        Task<RevenueStatsDto> GetRevenueStatsAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null);
        
        // Get order statistics for a business
        Task<OrderStatsDto> GetOrderStatsAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null);
        
        // Get top menu items by quantity sold
        Task<IEnumerable<TopMenuItemDto>> GetTopMenuItemsAsync(int businessId, int limit, DateTime startDate, DateTime endDate, int? branchId = null);
        
        // Get revenue breakdown by branch
        Task<IEnumerable<BranchRevenueDto>> GetRevenueByBranchAsync(int businessId, DateTime startDate, DateTime endDate);
        
        // Get low stock ingredients across all branches
        Task<IEnumerable<LowStockIngredientDto>> GetLowStockIngredientsAsync(int businessId);
        
        // Get recent orders
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int businessId, int limit, int? branchId = null);
        
        // Get revenue time series for chart
        Task<IEnumerable<RevenueTimeSeriesDto>> GetRevenueTimeSeriesAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null);
        
        // Get orders count by status
        Task<Dictionary<OrderStatus, int>> GetOrdersByStatusAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null);
    }

    
 




}
