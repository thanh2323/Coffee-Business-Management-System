using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        // Order-specific methods
        Task<IEnumerable<Order>> GetOrdersByBranchAsync(int branchId);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetOrdersByPaymentStatusAsync(PaymentStatus paymentStatus);
        Task<IEnumerable<Order>> GetOrdersByPaymentMethodAsync(PaymentMethod paymentMethod);

        // Business-wide (Owner)
        Task<IEnumerable<Order>> GetByBusinessIdAsync(int businessId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        // Order with related data
        Task<Order?> GetOrderWithItemsAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersWithItemsByCustomerIdAsync(int customerId);

        // OrderItem CRUD
        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);
        void AddOrderItem(OrderItem orderItem);
        void UpdateOrderItem(OrderItem orderItem);
        void DeleteOrderItem(OrderItem orderItem);
        void DeleteOrderItemsByOrderId(int orderId);
    }
}
