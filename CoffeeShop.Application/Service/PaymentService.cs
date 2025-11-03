using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CoffeeShop.Application.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITempOrderRepository _tempOrderRepo;
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentService(
            IUnitOfWork uow,
            ITempOrderRepository tempOrderRepo,
            IEnumerable<IPaymentGateway> gateways)
        {
            _uow = uow;
            _tempOrderRepo = tempOrderRepo;
            _gateways = gateways;
        }


        public async Task<PaymentLinkResult> CreatePaymentLinkAsync(int branchId, decimal amount, string description, PaymentGateway gateway, string reference)
        {
            var provider = _gateways.FirstOrDefault(g => g.Gateway == gateway);
            if (provider == null)
                return PaymentLinkResult.Failed("Unsupported payment gateway");

            return await provider.CreatePaymentLinkAsync(branchId, amount, description,reference);
        }

        public async Task<bool> VerifyPaymentAsync(IQueryCollection queryParams, PaymentGateway gateway)
        {
            var provider = _gateways.FirstOrDefault(g => g.Gateway == gateway);
            if (provider == null)
                return false;
            return await provider.VerifyPaymentAsync(queryParams);
        }

    }
}


