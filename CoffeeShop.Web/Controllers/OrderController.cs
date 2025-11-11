using CoffeeShop.Application.DTO;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "AllowAllEmployees")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _uow;
        private readonly IBranchService _branchService;
        private readonly IAuthService _authService;

        public OrderController(
            IOrderService orderService,
            IUnitOfWork uow,
            IBranchService branchService,
            IAuthService authService)
        {
            _orderService = orderService;
            _uow = uow;
            _branchService = branchService;
            _authService = authService;
        }

        // ✅ Index: Owner xem toàn bộ đơn hàng, Staff/Manager xem theo chi nhánh
        public async Task<IActionResult> Index(OrderStatus? status)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            IEnumerable<Order> orders;
            int? branchId = null;

            if (role == "Owner")
            {
                // ✅ Lấy thông tin Owner chính xác từ AuthService
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.BusinessId == null)
                {
                    return Json(new { success = false, message = "Owner or Business not found" });
                }

                // ✅ Lấy danh sách chi nhánh theo BusinessId
                var branches = await _branchService.GetByBusinessAsync(currentUser.BusinessId.Value);

                var allOrders = new List<Order>();
                foreach (var branch in branches)
                {
                    var result = await _orderService.GetOrdersByBranchAsync(status, branch.BranchId);
                    allOrders.AddRange(result.Orders);
                }

                orders = allOrders;
                ViewBag.BranchId = null;
            }
            else
            {
                // ✅ Giữ nguyên logic cho Staff/Manager
                var result = await _orderService.GetOrdersByBranchAsync(status);
                orders = result.Orders;
                branchId = result.BranchId;
                ViewBag.BranchId = branchId;
            }

            return View(orders.OrderByDescending(o => o.OrderDate));
        }

        // GET: /Order/Create
        public async Task<IActionResult> Create(int branchId)
        {
            ViewBag.BranchId = branchId;
            var menuItems = await _uow.MenuItems.GetByBranchIdAsync(branchId);
            ViewBag.MenuItems = menuItems.Where(m => m.IsAvailable).ToList();
            return View();
        }

        // POST: /Order/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            var result = await _orderService.CreateOrderAsync(
                request.BranchId,
                request.CustomerName,
                request.CustomerPhone,
                request.IsTakeAway,
                request.OrderItems.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            );

            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Message });

            return Json(new
            {
                success = true,
                orderId = result.Order!.OrderId,
                totalAmount = result.Order.TotalAmount,
                message = "Order created successfully"
            });
        }

        // POST: /Order/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            try
            {
                var staffId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var success = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, staffId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Status updated",
                        newStatus = newStatus.ToString()
                    });
                }

                return Json(new { success = false, message = "Failed to update" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems(int branchId)
        {
            var items = await _uow.MenuItems.GetByBranchIdAsync(branchId);
            return Json(items.Where(m => m.IsAvailable).Select(i => new
            {
                i.MenuItemId,
                i.Name,
                i.Price
            }));
        }
    }
}
