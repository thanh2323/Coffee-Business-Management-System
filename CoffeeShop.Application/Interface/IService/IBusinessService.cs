using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IBusinessService
    {
        Task<AdminResult> RegisterBusinessAsync(string businessName, string address, string? phone, int ownerId);
        Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, PaymentGateway gateway);
    }
}


