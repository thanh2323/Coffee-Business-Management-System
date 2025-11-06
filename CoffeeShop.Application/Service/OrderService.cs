using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrderRealtimeService _realtimeService;
        private readonly IAuthService _authService;

        public OrderService(IUnitOfWork uow, IAuthService authService, IOrderRealtimeService realtimeService)
        {
            _uow = uow;
            _authService = authService;
            _realtimeService = realtimeService;
        }

        public async Task<OrderResult> CreateOrderAsync(int branchId, string name, string? phone, bool isTakeAway, List<OrderItem> orderItems)
        {
            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null) return OrderResult.Failed("Branch not found");
            if (orderItems == null || !orderItems.Any()) return OrderResult.Failed("No items");

            var total = orderItems.Sum(x => x.Price * x.Quantity);
            var customer = new Customer
            {
                BranchId = branchId,
                Name = name,
                Phone = phone,
                Type = string.IsNullOrWhiteSpace(phone) ? CustomerType.Guest : CustomerType.Registered
            };

            var order = new Order
            {
                BranchId = branchId,
                Customer = customer,
                CurrentStatus = OrderStatus.Pending,
                IsTakeAway = isTakeAway,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.Cash,
                TotalAmount = total,
                OrderItems = orderItems
            };

            _uow.Orders.Add(order);
            await _uow.SaveChangesAsync();
            await _realtimeService.BroadcastNewOrderAsync(branchId, order);

            return OrderResult.Success(order);
        }

        // ✅ Manager / Staff: xem theo branch của họ
        public async Task<(IEnumerable<Order> Orders, int BranchId)> GetOrdersByBranchAsync(OrderStatus? status = null)
        {
            var user = await _authService.GetCurrentUserAsync();
            var branchId = user!.BranchId!.Value;

            var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
            if (status.HasValue)
                orders = orders.Where(o => o.CurrentStatus == status.Value);

            return (orders.OrderByDescending(o => o.OrderDate), branchId);
        }

        // ✅ Owner xem đơn hàng của từng chi nhánh
        public async Task<(IEnumerable<Order> Orders, int BranchId)> GetOrdersByBranchAsync(OrderStatus? status, int branchId)
        {
            var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
            if (status.HasValue)
                orders = orders.Where(o => o.CurrentStatus == status.Value);

            return (orders.OrderByDescending(o => o.OrderDate), branchId);
        }

        // ✅ Owner xem toàn bộ đơn hàng của toàn business
        public async Task<IEnumerable<Order>> GetOrdersByBusinessAsync(OrderStatus? status = null)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null || user.BusinessId == null)
                return Enumerable.Empty<Order>();

            // Lấy tất cả chi nhánh thuộc BusinessId của owner
            var branches = await _uow.Branches.GetByBusinessIdAsync(user.BusinessId.Value);
            var branchIds = branches.Select(b => b.BranchId).ToList();

            // Lấy tất cả đơn hàng của những chi nhánh đó
            var orders = await _uow.Orders.GetAllOrdersAsync();
            orders = orders.Where(o => branchIds.Contains(o.BranchId));

            if (status.HasValue)
                orders = orders.Where(o => o.CurrentStatus == status.Value);

            return orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }


        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int staffId)
        {
            var order = await _uow.Orders.GetOrderWithItemsAsync(orderId);
            if (order == null) return false;

            if (!OrderRules.CanChangeStatus(order.CurrentStatus, newStatus))
                throw new InvalidOperationException("Cannot change status");

            order.CurrentStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (newStatus == OrderStatus.Completed)
                order.PaymentStatus = PaymentStatus.Completed;

            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
            await _realtimeService.BroadcastOrderStatusAsync(orderId, newStatus.ToString(), order.BranchId);

            return true;
        }
    }
}
