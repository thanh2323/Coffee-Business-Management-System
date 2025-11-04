using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAuthService _authService;
        private readonly IBranchService _branchService;

        public DashboardController(
            IDashboardService dashboardService,
            IAuthService authService,
            IBranchService branchService)
        {
            _dashboardService = dashboardService;
            _authService = authService;
            _branchService = branchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? branchId = null, string? period = "today")
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null || user.BusinessId == null)
            {
                TempData["Error"] = "You don't have a business yet.";
                return RedirectToAction("Create", "Business");
            }

            var businessId = user.BusinessId.Value;

            // Calculate date range based on period
            var (startDate, endDate) = GetDateRange(period);

            // Load branches for filter dropdown
            var branches = await _branchService.GetBranchesForOwnerAsync(user.UserId);
            var ctx = await _branchService.GetOwnerContextAsync(user.UserId);

            // Load dashboard data
            var revenueStats = await _dashboardService.GetRevenueStatsAsync(businessId, startDate, endDate, branchId);
            var orderStats = await _dashboardService.GetOrderStatsAsync(businessId, startDate, endDate, branchId);
            var topMenuItems = await _dashboardService.GetTopMenuItemsAsync(businessId, 10, startDate, endDate, branchId);
            var revenueByBranch = await _dashboardService.GetRevenueByBranchAsync(businessId, startDate, endDate);
            var lowStockIngredients = await _dashboardService.GetLowStockIngredientsAsync(businessId);
            var recentOrders = await _dashboardService.GetRecentOrdersAsync(businessId, 10, branchId);
            var revenueTimeSeries = await _dashboardService.GetRevenueTimeSeriesAsync(businessId, startDate, endDate, branchId);
            var ordersByStatus = await _dashboardService.GetOrdersByStatusAsync(businessId, startDate, endDate, branchId);

            // Pass to view
            ViewBag.RevenueStats = revenueStats;
            ViewBag.OrderStats = orderStats;
            ViewBag.TopMenuItems = topMenuItems;
            ViewBag.RevenueByBranch = revenueByBranch;
            ViewBag.LowStockIngredients = lowStockIngredients;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.RevenueTimeSeries = revenueTimeSeries;
            ViewBag.OrdersByStatus = ordersByStatus;
            ViewBag.Branches = branches;
            ViewBag.SelectedBranchId = branchId;
            ViewBag.SelectedPeriod = period;
            ViewBag.BusinessName = ctx.businessName;
            ViewBag.OwnerName = ctx.ownerName;

            return View();
        }

        private (DateTime startDate, DateTime endDate) GetDateRange(string period)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var endDate = today.AddDays(1).AddTicks(-1); // End of today

            return period.ToLower() switch
            {
                "today" => (today, endDate),
                "week" => (today.AddDays(-6), endDate), // Last 7 days
                "month" => (today.AddDays(-29), endDate), // Last 30 days
                "quarter" => (today.AddMonths(-3), endDate),
                "year" => (today.AddYears(-1), endDate),
                _ => (today, endDate) // Default to today
            };
        }
    }
}
