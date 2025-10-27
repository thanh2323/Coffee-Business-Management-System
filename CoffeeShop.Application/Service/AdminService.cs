using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;

        public AdminService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AdminResult> ActivateBusinessAsync(int businessId)
        {
            try
            {
                var business = await _uow.Businesses.GetByIdAsync(businessId);
                if (business == null)
                    return AdminResult.Failed("Business not found");
                if (business.IsActive)
                    return AdminResult.Failed("Business is already active");
                business.IsActive = true;
                business.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
                _uow.Businesses.Update(business);
                await _uow.SaveChangesAsync();

                return AdminResult.Success(business, "Business activated successfully");
            }
            catch (Exception ex)
            {
                return AdminResult.Failed($"Activation failed: {ex.Message}");
            }
        }

        public async Task<AdminResult> DeactivateBusinessAsync(int businessId)
        {
            try
            {
                var business = await _uow.Businesses.GetByIdAsync(businessId);
                if (business == null)
                    return AdminResult.Failed("Business not found");
                if (!business.IsActive)
                    return AdminResult.Failed("Business is already inactive");
                business.IsActive = false;
                _uow.Businesses.Update(business);
                await _uow.SaveChangesAsync();

                return AdminResult.Success(business, "Business deactivated successfully");
            }
            catch (Exception ex)
            {
                return AdminResult.Failed($"Deactivation failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Business>> GetAllBusinessesAsync()
        {
            return await _uow.Businesses.GetAllAsync();
        }

       

        public async Task<IEnumerable<Business>> GetActiveBusinessesAsync()
        {
            var allBusinesses = await _uow.Businesses.GetAllAsync();
            return allBusinesses.Where(b => b.IsActive);
        }

        public async Task<IEnumerable<Business>> GetInactiveBusinessesAsync()
        {
            var allBusinesses = await _uow.Businesses.GetAllAsync();
            return allBusinesses.Where(b => !b.IsActive);
        }

        public async Task<AdminResult> UpdateSubscriptionAsync(int businessId, DateTime endDate, decimal monthlyFee)
        {
            try
            {
                var business = await _uow.Businesses.GetByIdAsync(businessId);
                if (business == null)
                    return AdminResult.Failed("Business not found");

                business.SubscriptionEndDate = endDate;
                business.MonthlyFee = monthlyFee;
                _uow.Businesses.Update(business);
                await _uow.SaveChangesAsync();

                return AdminResult.Success(business, "Subscription updated successfully");
            }
            catch (Exception ex)
            {
                return AdminResult.Failed($"Update failed: {ex.Message}");
            }
        }

        public async Task<AdminResult> CheckExpiredSubscriptionsAsync()
        {
            try
            {
                var allBusinesses = await _uow.Businesses.GetAllAsync();
                var expiredBusinesses = allBusinesses.Where(b =>
                    b.IsActive &&
                    b.SubscriptionEndDate.HasValue &&
                    b.SubscriptionEndDate < DateTime.UtcNow);

                int deactivatedCount = 0;
                foreach (var business in expiredBusinesses)
                {
                    business.IsActive = false;
                    _uow.Businesses.Update(business);
                    deactivatedCount++;
                }

                if (deactivatedCount > 0)
                {
                    await _uow.SaveChangesAsync();
                    return AdminResult.Success(null, $"{deactivatedCount} businesses deactivated due to expired subscription");
                }

                return AdminResult.Success(null, "No expired subscriptions found");
            }
            catch (Exception ex)
            {
                return AdminResult.Failed($"Check failed: {ex.Message}");
            }
        }
    }
}
