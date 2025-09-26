using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service.Gateways
{
    public class MoMoGateway : IPaymentGateway
    {
        public PaymentGateway Gateway => PaymentGateway.MoMo;

        public async Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, decimal amount, string description)
        {
            var reference = $"MOMO-{businessId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var url = $"https://test-payment.momo.vn/pay?ref={reference}&amount={amount}";
            return await Task.FromResult(PaymentLinkResult.Success(url, reference));
        }

        public async Task<bool> VerifyPaymentAsync(string reference)
        {
            // TODO: real verification with MoMo
            return await Task.FromResult(true);
        }
    }
}


