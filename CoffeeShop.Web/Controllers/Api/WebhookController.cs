using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Application.Interface.IService;

namespace CoffeeShop.Web.Controllers.Api
{
    [ApiController]
    [Route("api/webhooks/payments")]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IAdminService _adminService;

        public WebhookController(IPaymentService paymentService, IAdminService adminService)
        {
            _paymentService = paymentService;
            _adminService = adminService;
        }

        // POST api/webhooks/payments/confirm
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromForm] string reference, [FromForm] int businessId, [FromForm] string gateway)
        {
            var parsed = Enum.TryParse<CoffeeShop.Domain.Enums.PaymentGateway>(gateway, ignoreCase: true, out var gw);
            if (!parsed)
                return BadRequest(new { message = "Unsupported gateway" });

            var ok = await _paymentService.VerifyPaymentAsync(reference, gw);
            if (!ok)
                return BadRequest(new { message = "Verification failed" });

            // Verify the reference matches stored PaymentReference to avoid spoofing
            var business = await _adminService.GetBusinessByIdAsync(businessId);
            if (business == null || string.IsNullOrEmpty(business.PaymentReference) || !string.Equals(business.PaymentReference, reference, StringComparison.Ordinal))
            {
                return BadRequest(new { message = "Payment reference mismatch" });
            }

            var result = await _adminService.ActivateBusinessAsync(businessId);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Business activated" });
        }
    }
}


