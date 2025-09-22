using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service
{
    public class BusinessService : IBusinessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public BusinessService(IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<AdminResult> RegisterBusinessAsync(string businessName, string address, string? phone, int ownerId)
        {
            try
            {
                var owner = await _unitOfWork.Users.GetByIdAsync(ownerId);
                if (owner == null)
                    return AdminResult.Failed("Owner not found");
                if (owner.Role != UserRole.Owner)
                    return AdminResult.Failed("User is not an owner");
                if (owner.BusinessId.HasValue)
                    return AdminResult.Failed("Owner already has a business");

                var business = new Business
                {
                    Name = businessName,
                    Address = address,
                    Phone = phone,
                    IsActive = false,
                    MonthlyFee = 500000,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Businesses.Add(business);
                await _unitOfWork.SaveChangesAsync();

                owner.BusinessId = business.BusinessId;
                _unitOfWork.Users.Update(owner);
                await _unitOfWork.SaveChangesAsync();

                return AdminResult.Success(business, "Business registered successfully. Please complete payment to activate.");
            }
            catch (Exception ex)
            {
                return AdminResult.Failed($"Registration failed: {ex.Message}");
            }
        }

        public async Task<PaymentLinkResult> CreatePaymentLinkAsync(int businessId, PaymentGateway gateway)
        {
            var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
            if (business == null)
                return PaymentLinkResult.Failed("Business not found");

            var link = await _paymentService.CreatePaymentLinkAsync(businessId, business.MonthlyFee, $"Subscription for {business.Name}", gateway);
            if (!link.IsSuccess || string.IsNullOrEmpty(link.Reference))
                return link;

            business.PaymentReference = link.Reference;
            _unitOfWork.Businesses.Update(business);
            await _unitOfWork.SaveChangesAsync();

            return link;
        }
    }
}


