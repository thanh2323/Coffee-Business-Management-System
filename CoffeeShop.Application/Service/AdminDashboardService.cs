using CoffeeShop.Application.DTO;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _uow;

        public AdminDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AdminSystemStatsDto> GetSystemStatsAsync(DateTime startDate, DateTime endDate)
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            var allBranches = new List<Branch>();
            
            foreach (var business in businesses)
            {
                var branches = await _uow.Branches.GetByBusinessIdAsync(business.BusinessId);
                allBranches.AddRange(branches);
            }

            var branchIds = allBranches.Select(b => b.BranchId).ToList();
            var allOrders = new List<Order>();

            foreach (var branchId in branchIds)
            {
                var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
                allOrders.AddRange(orders);
            }

            var filteredOrders = allOrders
                .Where(o => o.OrderDate.Date >= startDate.Date && o.OrderDate.Date <= endDate.Date)
                .ToList();

            var completedOrders = filteredOrders
                .Where(o => o.PaymentStatus == PaymentStatus.Completed)
                .ToList();

            var today = DateTime.UtcNow.Date;
            var todayOrders = filteredOrders.Where(o => o.OrderDate.Date == today).ToList();
            var todayRevenue = completedOrders
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.TotalAmount);

            var users = await _uow.Users.GetAllAsync();
            var expiringBusinesses = businesses.Where(b =>
                b.IsActive &&
                b.SubscriptionEndDate.HasValue &&
                b.SubscriptionEndDate.Value.Date <= today.AddDays(30) &&
                b.SubscriptionEndDate.Value.Date >= today)
                .ToList();

            return new AdminSystemStatsDto
            {
                TotalBusinesses = businesses.Count(),
                ActiveBusinesses = businesses.Count(b => b.IsActive),
                InactiveBusinesses = businesses.Count(b => !b.IsActive),
                TotalBranches = allBranches.Count,
                TotalRevenue = completedOrders.Sum(o => o.TotalAmount),
                TodayRevenue = todayRevenue,
                TotalOrders = filteredOrders.Count,
                TodayOrders = todayOrders.Count,
                TotalUsers = users.Count(),
                ExpiringSubscriptions = expiringBusinesses.Count
            };
        }

        public async Task<AdminBusinessStatsDto> GetBusinessStatsAsync()
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            var today = DateTime.UtcNow;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var newBusinessesThisMonth = businesses
                .Where(b => b.CreatedAt >= startOfMonth)
                .Count();

            var expiringBusinesses = businesses.Where(b =>
                b.IsActive &&
                b.SubscriptionEndDate.HasValue &&
                b.SubscriptionEndDate.Value.Date <= today.AddDays(30) &&
                b.SubscriptionEndDate.Value.Date >= today.Date)
                .ToList();

            return new AdminBusinessStatsDto
            {
                TotalBusinesses = businesses.Count(),
                ActiveBusinesses = businesses.Count(b => b.IsActive),
                InactiveBusinesses = businesses.Count(b => !b.IsActive),
                NewBusinessesThisMonth = newBusinessesThisMonth,
                BusinessesWithExpiringSubscription = expiringBusinesses.Count
            };
        }

        public async Task<IEnumerable<TopBusinessDto>> GetTopBusinessesAsync(int limit, DateTime startDate, DateTime endDate)
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            var topBusinesses = new List<TopBusinessDto>();

            foreach (var business in businesses)
            {
                var branches = await _uow.Branches.GetByBusinessIdAsync(business.BusinessId);
                var branchIds = branches.Select(b => b.BranchId).ToList();

                var businessOrders = new List<Order>();
                foreach (var branchId in branchIds)
                {
                    var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
                    businessOrders.AddRange(orders);
                }

                var filteredOrders = businessOrders
                    .Where(o => o.OrderDate.Date >= startDate.Date && 
                               o.OrderDate.Date <= endDate.Date &&
                               o.PaymentStatus == PaymentStatus.Completed)
                    .ToList();

                var revenue = filteredOrders.Sum(o => o.TotalAmount);
                var orderCount = filteredOrders.Count();
                var avgOrderValue = orderCount > 0 ? revenue / orderCount : 0;

                topBusinesses.Add(new TopBusinessDto
                {
                    BusinessId = business.BusinessId,
                    BusinessName = business.Name,
                    Revenue = revenue,
                    OrderCount = orderCount,
                    BranchCount = branches.Count(),
                    AverageOrderValue = avgOrderValue
                });
            }

            return topBusinesses
                .OrderByDescending(b => b.Revenue)
                .Take(limit)
                .ToList();
        }

        public async Task<IEnumerable<ExpiringBusinessDto>> GetExpiringBusinessesAsync(int daysAhead = 30)
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            var today = DateTime.UtcNow.Date;
            var expiryDate = today.AddDays(daysAhead);

            var expiringBusinesses = businesses
                .Where(b => b.SubscriptionEndDate.HasValue &&
                           b.SubscriptionEndDate.Value.Date >= today &&
                           b.SubscriptionEndDate.Value.Date <= expiryDate)
                .Select(b => new ExpiringBusinessDto
                {
                    BusinessId = b.BusinessId,
                    BusinessName = b.Name,
                    SubscriptionEndDate = b.SubscriptionEndDate,
                    DaysUntilExpiry = (int)(b.SubscriptionEndDate!.Value.Date - today).TotalDays,
                    IsActive = b.IsActive
                })
                .OrderBy(b => b.SubscriptionEndDate)
                .ToList();

            return expiringBusinesses;
        }

        public async Task<IEnumerable<Business>> GetRecentBusinessesAsync(int limit)
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            return businesses
                .OrderByDescending(b => b.CreatedAt)
                .Take(limit)
                .ToList();
        }

        public async Task<IEnumerable<RevenueTimeSeriesDto>> GetSystemRevenueTimeSeriesAsync(DateTime startDate, DateTime endDate)
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            var allBranches = new List<Branch>();

            foreach (var business in businesses)
            {
                var branches = await _uow.Branches.GetByBusinessIdAsync(business.BusinessId);
                allBranches.AddRange(branches);
            }

            var branchIds = allBranches.Select(b => b.BranchId).ToList();
            var allOrders = new List<Order>();

            foreach (var branchId in branchIds)
            {
                var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
                allOrders.AddRange(orders);
            }

            var filteredOrders = allOrders
                .Where(o => o.OrderDate.Date >= startDate.Date && 
                           o.OrderDate.Date <= endDate.Date &&
                           o.PaymentStatus == PaymentStatus.Completed)
                .ToList();

            var timeSeries = filteredOrders
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

        public async Task<Dictionary<OrderStatus, int>> GetSystemOrdersByStatusAsync(DateTime startDate, DateTime endDate)
        {
            var businesses = await _uow.Businesses.GetAllAsync();
            var allBranches = new List<Branch>();

            foreach (var business in businesses)
            {
                var branches = await _uow.Branches.GetByBusinessIdAsync(business.BusinessId);
                allBranches.AddRange(branches);
            }

            var branchIds = allBranches.Select(b => b.BranchId).ToList();
            var allOrders = new List<Order>();

            foreach (var branchId in branchIds)
            {
                var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
                allOrders.AddRange(orders);
            }

            var filteredOrders = allOrders
                .Where(o => o.OrderDate.Date >= startDate.Date && o.OrderDate.Date <= endDate.Date)
                .ToList();

            return filteredOrders
                .GroupBy(o => o.CurrentStatus)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
