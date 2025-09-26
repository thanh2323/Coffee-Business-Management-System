using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IPaymentGateway
    {
        PaymentGateway Gateway { get; }
        Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, decimal amount, string description);
        Task<bool> VerifyPaymentAsync(string reference);
    }
}


