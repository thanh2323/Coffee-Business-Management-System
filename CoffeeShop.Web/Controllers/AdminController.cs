using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Application.Interface.IService;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminController(IAdminService adminService, IAdminDashboardService adminDashboardService)
        {
            _adminService = adminService;
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string? period = "today")
        {
            // Calculate date range based on period
            var (startDate, endDate) = GetDateRange(period!);

            // Load dashboard data
            var systemStats = await _adminDashboardService.GetSystemStatsAsync(startDate, endDate);
            var businessStats = await _adminDashboardService.GetBusinessStatsAsync();
            var topBusinesses = await _adminDashboardService.GetTopBusinessesAsync(10, startDate, endDate);
            var expiringBusinesses = await _adminDashboardService.GetExpiringBusinessesAsync(30);
            var recentBusinesses = await _adminDashboardService.GetRecentBusinessesAsync(10);
            var revenueTimeSeries = await _adminDashboardService.GetSystemRevenueTimeSeriesAsync(startDate, endDate);
            var ordersByStatus = await _adminDashboardService.GetSystemOrdersByStatusAsync(startDate, endDate);

            // Pass to view
            ViewBag.SystemStats = systemStats;
            ViewBag.BusinessStats = businessStats;
            ViewBag.TopBusinesses = topBusinesses;
            ViewBag.ExpiringBusinesses = expiringBusinesses;
            ViewBag.RecentBusinesses = recentBusinesses;
            ViewBag.RevenueTimeSeries = revenueTimeSeries;
            ViewBag.OrdersByStatus = ordersByStatus;
            ViewBag.SelectedPeriod = period;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var all = await _adminService.GetAllBusinessesAsync();
            return View(all);
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

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _adminService.ActivateBusinessAsync(id);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _adminService.DeactivateBusinessAsync(id);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }

        public IActionResult Package()
        {
            return View();
        }

        public IActionResult Post()
        {
            return View();
        }



    }
}


