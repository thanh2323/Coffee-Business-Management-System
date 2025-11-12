using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        // Order-specific methods
        Task<IEnumerable<Order>> GetOrdersByBranchAsync(int branchId);
       
        
        // Order with OrderItems (include related data)
        Task<Order?> GetOrderWithItemsAsync(int orderId);
        // OrderItem CRUD methods (since OrderItem is part of Order aggregate)
      
        void AddOrderItem(OrderItem orderItem);
        void UpdateOrderItem(OrderItem orderItem);
        void DeleteOrderItem(OrderItem orderItem);
        void DeleteOrderItemsByOrderId(int orderId);
    }
}