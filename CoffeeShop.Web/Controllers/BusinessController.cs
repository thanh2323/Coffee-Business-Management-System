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
        public BusinessController(IBusinessService businessService, IAuthService authService, IPaymentService paymentService)
        {
            _businessService = businessService;
            _authService = authService;
            _paymentService = paymentService;
         
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> My()
        {

            var user = await _authService.GetCurrentUserAsync();
            if (user == null || user.BusinessId == null)
            {
                TempData["Error"] = "You don't have a business yet.";
                return RedirectToAction("Create");
            }

            var business = await _businessService.GetBusinessByIdAsync(user.BusinessId.Value);
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

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                TempData["Error"] = "Invalid user.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Business name is required.";
                return View();
            }

            var result = await _businessService.RegisterBusinessAsync(name, address, phone, user.UserId);
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
        public async Task<IActionResult> Pay(int businessId, PaymentGateway gateway)
        {
            var business = await _businessService.GetBusinessByIdAsync(businessId);
            if (business == null)
            {
                TempData["Error"] = "Business not found.";
                return RedirectToAction("Create");
            }
    
            var paymentResult = await _paymentService.CreatePaymentLinkAsync(businessId, business.MonthlyFee, $"Subscription for {business.Name}", gateway, business.PaymentReference!);

            if (!paymentResult.IsSuccess || string.IsNullOrEmpty(paymentResult.PaymentUrl))
            {
                TempData["Error"] = paymentResult.Message;
                return RedirectToAction("Success", new { id = businessId });
            }

            return Redirect(paymentResult.PaymentUrl);
        }
    }
}


