using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IOrderItemRepository : IBaseRepository<OrderItem>
    {
        // OrderItem-specific methods can be added here
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderItem>> GetByMenuItemIdAsync(int menuItemId);
    }
}