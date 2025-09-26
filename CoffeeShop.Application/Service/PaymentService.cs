using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentService(IUnitOfWork unitOfWork, IEnumerable<IPaymentGateway> gateways)
        {
            _unitOfWork = unitOfWork;
            _gateways = gateways;
        }

        public async Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, decimal amount, string description, PaymentGateway gateway)
        {
            var provider = _gateways.FirstOrDefault(g => g.Gateway == gateway);
            if (provider == null)
                return PaymentLinkResult.Failed("Unsupported payment gateway");

            return await provider.CreatePaymentLinkAsync(businessId, amount, description);
        }

        public async Task<bool> VerifyPaymentAsync(string reference, PaymentGateway gateway)
        {
            var provider = _gateways.FirstOrDefault(g => g.Gateway == gateway);
            if (provider == null)
                return false;
            return await provider.VerifyPaymentAsync(reference);
        }
    }
}


