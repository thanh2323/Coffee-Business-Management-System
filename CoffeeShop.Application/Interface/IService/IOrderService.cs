using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IOrderService
    {
        Task<OrderResult> CreateOrderAsync(int branchId, string name, string? phone, bool isTakeAway, List<OrderItem> orderItems);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int staffId);
        Task<(IEnumerable<Order> Orders, int BranchId)> GetOrdersByBranchAsync(OrderStatus? status = null);
        Task<(IEnumerable<Order> Orders, int BranchId)> GetOrdersByBranchAsync(OrderStatus? status, int branchId);
        Task<IEnumerable<Order>> GetOrdersByBusinessAsync(OrderStatus? status = null);
    }

    public class OrderResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Order? Order { get; set; }

        public static OrderResult Success(Order order, string message = "Order processed successfully")
            => new() { IsSuccess = true, Order = order, Message = message };

        public static OrderResult Failed(string message)
            => new() { IsSuccess = false, Message = message };
    }
}
