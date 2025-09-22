using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Application.Interface.IService;
using System.Security.Claims;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class BusinessController : Controller
    {
        private readonly IBusinessService _businessService;
        private readonly IAuthService _authService;
        private readonly IPaymentService _paymentService;
        private readonly IAdminService _adminService;

        public BusinessController(IBusinessService businessService, IAuthService authService, IPaymentService paymentService, IAdminService adminService)
        {
            _businessService = businessService;
            _authService = authService;
            _paymentService = paymentService;
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> My()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Create");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null || user.BusinessId == null)
            {
                TempData["Error"] = "You don't have a business yet.";
                return RedirectToAction("Create");
            }

            var business = await _adminService.GetBusinessByIdAsync(user.BusinessId.Value);
            if (business == null)
            {
                TempData["Error"] = "Business not found.";
                return RedirectToAction("Create");
            }

            return View(business);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string address, string? phone)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Business name is required.";
                return View();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return View();
            }

            var result = await _businessService.RegisterBusinessAsync(name, address, phone, ownerId);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View();
            }

            TempData["Success"] = "Business created. Please complete payment to activate.";
            return RedirectToAction("Success", new { id = result.Business!.BusinessId });
        }

        [HttpGet]
        public IActionResult Success(int id)
        {
            ViewBag.BusinessId = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Pay(int businessId, string gateway = "VNPay")
        {
            var business = await _adminService.GetBusinessByIdAsync(businessId);
            if (business == null)
            {
                TempData["Error"] = "Business not found.";
                return RedirectToAction("Create");
            }

            var parsed = Enum.TryParse<PaymentGateway>(gateway, true, out var gw) ? gw : PaymentGateway.VNPay;
            var link = await _businessService.CreatePaymentLinkAsync(businessId, parsed);
            if (!link.IsSuccess || string.IsNullOrEmpty(link.PaymentUrl))
            {
                TempData["Error"] = link.Message;
                return RedirectToAction("Success", new { id = businessId });
            }

            return Redirect(link.PaymentUrl);
        }
    }
}


