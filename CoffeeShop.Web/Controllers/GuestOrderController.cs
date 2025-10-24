using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    public class GuestOrderController : Controller
    {
        private readonly IGuestOrderService _guestOrderService;
        private readonly IPaymentService _paymentService;

        public GuestOrderController(
            IGuestOrderService guestOrderService,
            IPaymentService paymentService)
        {
            _guestOrderService = guestOrderService;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();

            var result = await _guestOrderService.GetMenuForTableAsync(tableId, branchId);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Table = result.Table;
            ViewBag.Branch = result.Branch;

            return View(result.MenuItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int menuItemId, int quantity, int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();
            var result = await _guestOrderService.AddToCartAsync(sessionId, menuItemId, quantity, branchId, tableId);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = "Added to cart" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int menuItemId, int quantity, int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();
            var result = await _guestOrderService.UpdateCartAsync(sessionId, menuItemId, quantity);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int menuItemId, int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();
            var result = await _guestOrderService.RemoveFromCartAsync(sessionId, menuItemId);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = "Item removed from cart" });
        }

        [HttpGet]
        public async Task<IActionResult> Cart(int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();

            var tableResult = await _guestOrderService.GetMenuForTableAsync(tableId, branchId);
            if (!tableResult.IsSuccess)
            {
                TempData["Error"] = tableResult.Message;
                return RedirectToAction("Index", "Home");
            }

            var cartResult = await _guestOrderService.GetCartDetailsAsync(sessionId);

            if (!cartResult.IsSuccess)
            {
                TempData["Error"] = cartResult.Message;
                return RedirectToAction("Index", new { tableId, branchId });
            }

            ViewBag.Table = tableResult.Table;
            ViewBag.Branch = tableResult.Branch;
            ViewBag.Cart = cartResult.CartItems;
            ViewBag.TotalAmount = cartResult.TotalAmount;

            return View(cartResult.CartItems);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();
            var cartResult = await _guestOrderService.GetCartDetailsAsync(sessionId);

            if (!cartResult.IsSuccess)
            {
                TempData["Error"] = cartResult.Message;
                return RedirectToAction("Index", new { tableId, branchId });
            }

            var tableResult = await _guestOrderService.GetMenuForTableAsync(tableId, branchId);
            if (!tableResult.IsSuccess)
            {
                TempData["Error"] = tableResult.Message;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Table = tableResult.Table;
            ViewBag.Branch = tableResult.Branch;
            ViewBag.Cart = cartResult.CartItems;
            ViewBag.TotalAmount = cartResult.TotalAmount;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string customerName, string? customerPhone, int tableId, int branchId)
        {
            var sessionId = GetOrCreateSessionId();
            var result = await _guestOrderService.CreateTempOrderAsync(customerName, customerPhone, tableId, branchId, sessionId);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Checkout", new { tableId, branchId });
            }

            // Generate payment link
            var paymentResult = await _paymentService.CreatePaymentLinkAsync(
                branchId,
                result.TempOrder!.PayableAmount,
                $"Order for {customerName}",
                Domain.Enums.PaymentGateway.VNPay);

            if (!paymentResult.IsSuccess)
            {
                TempData["Error"] = "Failed to create payment link";
                return RedirectToAction("Checkout", new { tableId, branchId });
            }

            // Clear cart
            await _guestOrderService.ClearCartAsync(sessionId);

            // Redirect to payment
            return Redirect(paymentResult.PaymentUrl!);
        }

        private string GetOrCreateSessionId()
        {
            var sessionId = HttpContext.Session.GetString("GuestSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("GuestSessionId", sessionId);
            }
            return sessionId;
        }
    }
}
