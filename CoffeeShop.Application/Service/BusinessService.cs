using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service
{
    public class BusinessService : IBusinessService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPaymentService _paymentService;

        public BusinessService(IUnitOfWork uow, IPaymentService paymentService)
        {
            _uow = uow;
            _paymentService = paymentService;
        }

        public async Task<Business?> GetBusinessByIdAsync(int businessId)
        {
            return await _uow.Businesses.GetByIdAsync(businessId);
        }

        public async Task<AdminResult> RegisterBusinessAsync(string businessName, string address, string? phone, int ownerId)
        {
            try
            {
                var owner = await _uow.Users.GetByIdAsync(ownerId);
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
                    CreatedAt = DateTime.UtcNow,
                    PaymentReference = $"BIZ-{Guid.NewGuid()}"
                };

                _uow.Businesses.Add(business);
                await _uow.SaveChangesAsync();

                owner.BusinessId = business.BusinessId;
                _uow.Users.Update(owner);
                await _uow.SaveChangesAsync();

                return AdminResult.Success(business, "Business registered successfully. Please complete payment to activate.");
            }
            catch (Exception ex)
            {
                return AdminResult.Failed($"Registration failed: {ex.Message}");
            }
        }
        public async Task<bool> CompletePaymentAsync(string refCode, PaymentGateway gateway)
        {
            try
            {
                var bu = await _uow.Businesses.GetAllAsync();
                var business = (await _uow.Businesses.GetAllAsync()).FirstOrDefault(b => b.PaymentReference == refCode);
               
                if (business == null)
                    return false;

                if (business.IsActive)
                    return true; // tránh double update

                business.IsActive = true;
                business.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);

                _uow.Businesses.Update(business);
                await _uow.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CompletePaymentAsync] Error: {ex.Message}");
                return false;
            }
        }


    }
}


