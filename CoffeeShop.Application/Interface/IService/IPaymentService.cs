namespace CoffeeShop.Application.Interface.IService
{
    public interface IPaymentService
    {
        Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, decimal amount, string description, CoffeeShop.Domain.Enums.PaymentGateway gateway);
        Task<bool> VerifyPaymentAsync(string reference, CoffeeShop.Domain.Enums.PaymentGateway gateway);
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


