using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service.Gateways
{
    public class VNPayGateway : IPaymentGateway
    {
        public PaymentGateway Gateway => PaymentGateway.VNPay;

        public async Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, decimal amount, string description)
        {
            var reference = $"VNPAY-{businessId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var url = $"https://sandbox.vnpay.vn/pay?ref={reference}&amount={amount}";
            return await Task.FromResult(PaymentLinkResult.Success(url, reference));
        }

        public async Task<bool> VerifyPaymentAsync(string reference)
        {
            // TODO: real verification with VNPay
            return await Task.FromResult(true);
        }
    }
}


