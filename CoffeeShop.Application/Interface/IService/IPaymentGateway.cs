using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IPaymentGateway
    {
        PaymentGateway Gateway { get; }
        Task<PaymentLinkResult> CreatePaymentLinkAsync(int branchId, decimal amount, string description, string orderId);
        Task<bool> VerifyPaymentAsync(IQueryCollection queryParams);
    }
}


