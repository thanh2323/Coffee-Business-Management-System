using CoffeeShop.Application.DTO;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Rules;


namespace CoffeeShop.Application.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _authService;

        public DashboardService(IUnitOfWork uow, IAuthService authService)
        {
            _uow = uow;
            _authService = authService;
        }

        public async Task<RevenueStatsDto> GetRevenueStatsAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null)
        {
            var orders = await GetFilteredOrdersAsync(businessId, startDate, endDate, branchId);
            var completedOrders = orders.Where(o => o.PaymentStatus == PaymentStatus.Completed);

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var todayRevenue = completedOrders
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.TotalAmount);

            var yesterdayRevenue = completedOrders
                .Where(o => o.OrderDate.Date == yesterday)
                .Sum(o => o.TotalAmount);

            var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
            var totalOrders = completedOrders.Count();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var percentageChange = yesterdayRevenue > 0
                ? ((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100
                : (todayRevenue > 0 ? 100 : 0);

            return new RevenueStatsDto
            {
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                YesterdayRevenue = yesterdayRevenue,
                PercentageChange = percentageChange,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<OrderStatsDto> GetOrderStatsAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null)
        {
            var orders = await GetFilteredOrdersAsync(businessId, startDate, endDate, branchId);
            var today = DateTime.UtcNow.Date;

            var todayOrders = orders.Count(o => o.OrderDate.Date == today);
            var completedOrders = orders.Count(o => o.CurrentStatus == OrderStatus.Completed);
            var pendingOrders = orders.Count(o => o.CurrentStatus == OrderStatus.Pending || 
                                                  o.CurrentStatus == OrderStatus.Confirmed || 
                                                  o.CurrentStatus == OrderStatus.Preparing || 
                                                  o.CurrentStatus == OrderStatus.Ready);

            var ordersByStatus = orders
                .GroupBy(o => o.CurrentStatus)
                .ToDictionary(g => g.Key, g => g.Count());

            return new OrderStatsDto
            {
                TotalOrders = orders.Count(),
                TodayOrders = todayOrders,
                OrdersByStatus = ordersByStatus,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders
            };
        }

        public async Task<IEnumerable<TopMenuItemDto>> GetTopMenuItemsAsync(int businessId, int limit, DateTime startDate, DateTime endDate, int? branchId = null)
        {
            var orders = await GetFilteredOrdersAsync(businessId, startDate, endDate, branchId);
            var completedOrders = orders.Where(o => o.PaymentStatus == PaymentStatus.Completed);

            var menuItemStats = completedOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem.Name, oi.MenuItem.Category })
                .Select(g => new TopMenuItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    MenuItemName = g.Key.Name,
                    Category = g.Key.Category ?? "Uncategorized",
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(limit)
                .ToList();

            return menuItemStats;
        }

        public async Task<IEnumerable<BranchRevenueDto>> GetRevenueByBranchAsync(int businessId, DateTime startDate, DateTime endDate)
        {
            var branches = await _uow.Branches.GetByBusinessIdAsync(businessId);
            var branchRevenues = new List<BranchRevenueDto>();

            foreach (var branch in branches)
            {
                var branchOrders = await _uow.Orders.GetOrdersByBranchAsync(branch.BranchId);
                var filteredOrders = branchOrders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && 
                               o.PaymentStatus == PaymentStatus.Completed);

                var revenue = filteredOrders.Sum(o => o.TotalAmount);
                var orderCount = filteredOrders.Count();
                var avgOrderValue = orderCount > 0 ? revenue / orderCount : 0;

                branchRevenues.Add(new BranchRevenueDto
                {
                    BranchId = branch.BranchId,
                    BranchName = branch.Name,
                    Revenue = revenue,
                    OrderCount = orderCount,
                    AverageOrderValue = avgOrderValue
                });
            }

            return branchRevenues.OrderByDescending(br => br.Revenue);
        }

        public async Task<IEnumerable<LowStockIngredientDto>> GetLowStockIngredientsAsync(int businessId)
        {
            var branches = await _uow.Branches.GetByBusinessIdAsync(businessId);
            var lowStockIngredients = new List<LowStockIngredientDto>();

            foreach (var branch in branches)
            {
                var ingredients = await _uow.Ingredients.GetIngredientsByBranchAsync(branch.BranchId);
                var lowStock = ingredients.Where(i => InventoryRules.IsLowStock(i));

                lowStockIngredients.AddRange(lowStock.Select(i => new LowStockIngredientDto
                {
                    IngredientId = i.IngredientId,
                    IngredientName = i.Name,
                    BranchId = branch.BranchId,
                    BranchName = branch.Name,
                    Quantity = i.Quantity,
                    BaseUnit = i.BaseUnit
                }));
            }

            return lowStockIngredients.OrderBy(l => l.Quantity);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int businessId, int limit, int? branchId = null)
        {
            var orders = await GetFilteredOrdersAsync(businessId, DateTime.MinValue, DateTime.UtcNow, branchId);

            return orders
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .ToList();
        }

        public async Task<IEnumerable<RevenueTimeSeriesDto>> GetRevenueTimeSeriesAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null)
        {
            var orders = await GetFilteredOrdersAsync(businessId, startDate, endDate, branchId);
            var completedOrders = orders.Where(o => o.PaymentStatus == PaymentStatus.Completed);

            var timeSeries = completedOrders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenueTimeSeriesDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(ts => ts.Date)
                .ToList();

            // Fill in missing dates with zero revenue
            var allDates = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
                .Select(i => startDate.Date.AddDays(i))
                .ToList();

            var result = allDates
                .Select(date => timeSeries.FirstOrDefault(ts => ts.Date == date) ?? new RevenueTimeSeriesDto
                {
                    Date = date,
                    Revenue = 0,
                    OrderCount = 0
                })
                .ToList();

            return result;
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrdersByStatusAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId = null)
        {
            var orders = await GetFilteredOrdersAsync(businessId, startDate, endDate, branchId);

            return orders
                .GroupBy(o => o.CurrentStatus)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // Helper method to get filtered orders
        private async Task<IEnumerable<Order>> GetFilteredOrdersAsync(int businessId, DateTime startDate, DateTime endDate, int? branchId)
        {
            var branches = await _uow.Branches.GetByBusinessIdAsync(businessId);
            
            // Verify branch belongs to business if branchId is specified
            if (branchId.HasValue)
            {
                var branch = branches.FirstOrDefault(b => b.BranchId == branchId.Value);
                if (branch == null)
                    return Enumerable.Empty<Order>();
            }

            var branchIds = branchId.HasValue ? new List<int> { branchId.Value } : branches.Select(b => b.BranchId).ToList();

            if (!branchIds.Any())
                return Enumerable.Empty<Order>();

            var allOrders = new List<Order>();

            foreach (var id in branchIds)
            {
                var branchOrders = await _uow.Orders.GetOrdersByBranchAsync(id);
                allOrders.AddRange(branchOrders);
            }

            return allOrders
                .Where(o => o.OrderDate.Date >= startDate.Date && o.OrderDate.Date <= endDate.Date)
                .ToList();
        }
    }
}
