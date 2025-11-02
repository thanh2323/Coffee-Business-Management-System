using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IOrderRealtimeService
    {
        Task BroadcastNewOrderAsync(int branchId, Order order);
        Task BroadcastOrderStatusAsync(int orderId, string newStatus, int branchId);
    }
}
