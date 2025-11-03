using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IPaymentService
    {
        Task<PaymentLinkResult> CreatePaymentLinkAsync(int branhid, decimal amount, string description, PaymentGateway gateway, string reference);
        Task<bool> VerifyPaymentAsync(IQueryCollection queryParams, PaymentGateway gateway);

    }

    public class PaymentLinkResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string? Reference { get; set; }

        public static PaymentLinkResult Success(string url, string reference)
        {
            return new PaymentLinkResult { IsSuccess = true, PaymentUrl = url, Reference = reference, Message = "Created" };
        }

        public static PaymentLinkResult Failed(string message)
        {
            return new PaymentLinkResult { IsSuccess = false, Message = message };
        }
    }
}


