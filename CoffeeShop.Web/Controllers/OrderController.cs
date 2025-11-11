// Controllers/OrderController.cs
using CoffeeShop.Application.DTO;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "StaffOrManager")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _uow;

        public OrderController(IOrderService orderService, IUnitOfWork uow)
        {
            _orderService = orderService;
            _uow = uow;
        }

        // GET: /Order/Index
        public async Task<IActionResult> Index(OrderStatus? status)
        {
            var result = await _orderService.GetOrdersByBranchAsync(status);
            ViewBag.BranchId = result.BranchId;
            return View(result.Orders);
        }

        // GET: /Order/Create
        public async Task<IActionResult> Create(int branchId)
        {
            ViewBag.BranchId = branchId;

            // Load menu items trực tiếp
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
                request.OrderItems
                    .Select(i => new OrderItem
                    {
                        MenuItemId = i.MenuItemId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                    })
                    .ToList()
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

        // GET: /Order/GetMenuItems (cho AJAX nếu cần)
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