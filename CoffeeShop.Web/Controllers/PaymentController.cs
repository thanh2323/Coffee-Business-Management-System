using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderPaymentService _orderPaymentService;
        private readonly IBusinessService _businessService;

        public PaymentController(IPaymentService paymentService, IOrderPaymentService orderPaymentService, IBusinessService businessService)
        {
            _paymentService = paymentService;
            _orderPaymentService = orderPaymentService;
            _businessService = businessService;
        }

        [HttpGet("payment/return")]
        public async Task<IActionResult> Return()
        {
            var query = HttpContext.Request.Query;

            // 1️⃣ Detect gateway from query keys
            var gateway = DetectGateway(query);

            // 2️⃣ Verify payment
            bool isValid = await _paymentService.VerifyPaymentAsync(query, gateway);
            if (!isValid)
            {
                ViewBag.Message = $"{gateway} payment failed!";
                return View("Fail");
            }

            // 3️⃣ Extract order reference based on gateway
            string refCode = ExtractOrderReference(query, gateway);
            if (string.IsNullOrWhiteSpace(refCode))
            {
                ViewBag.Message = "Invalid order reference!";
                return View("Fail");
            }

            if (refCode.StartsWith("BIZ-"))
            {
                bool bizOk = await _businessService.CompletePaymentAsync(refCode, gateway);
                ViewBag.Message = bizOk
                    ? $"{gateway} business payment successful!"
                    : "Payment ok but failed to update business.";
                return View(bizOk ? "Success" : "Fail");
            }
            else if (refCode.StartsWith("ORD-"))
            {
                bool orderOk = await _orderPaymentService.ConvertTempOrderToRealOrderAsync(refCode, gateway);
                ViewBag.Message = orderOk
                    ? $"{gateway} order payment successful!"
                    : "Payment ok but failed to create order.";
                return View(orderOk ? "Success" : "Fail");
            }

            ViewBag.Message = "Unknown reference type!";
            return View("Fail");
        }


        /* // Optional webhook
         [HttpPost("payment/webhook/{gateway}")]
         public async Task<IActionResult> Webhook([FromRoute] PaymentGateway gateway, [FromBody] object payload)
         {
             // tương lai nếu gateway gửi thông báo server-to-server
             //  có thể xử lý ở đây
             return Ok();
         }

 */

        // Helper
        private PaymentGateway DetectGateway(IQueryCollection query)
        {
            if (query.Keys.Any(k => k.StartsWith("vnp_")))
                return PaymentGateway.VNPay;
            if (query.Keys.Any(k => k.StartsWith("momo_")))
                return PaymentGateway.MoMo;
           return PaymentGateway.VNPay; // default


        }

        private string ExtractOrderReference(IQueryCollection query, PaymentGateway gateway)
        {
            switch (gateway)
            {
                case PaymentGateway.VNPay:
                   
                    return query["vnp_TxnRef"].ToString();

                case PaymentGateway.MoMo:
                    
                    return query["orderId"].ToString()
                           ?? query["momo_orderId"].ToString()
                           ?? query["requestId"].ToString();

                default:
                    return string.Empty;
            }
        }
    }
}
