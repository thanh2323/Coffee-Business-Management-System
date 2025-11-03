using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface ITempOrderRepository
    {
        Task<TempOrder?> GetAsync(string tempOrderId);
        Task<bool> SetAsync(TempOrder tempOrder, TimeSpan? expiry = null);
        Task<bool> DeleteAsync(string tempOrderId);
        Task<bool> ExistsAsync(string tempOrderId);
        Task SetIdempotencyAsync(string reference, TimeSpan? expiry = null);
        Task<bool> IsIdempotentAsync(string reference);
    }
}
