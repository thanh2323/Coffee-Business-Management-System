using CoffeeShop.Application.Interface;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Rules;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Service
{

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrderRealtimeService _realtimeService;
        private readonly IAuthService _authService;
        private readonly IBranchResolverService _branchResolver;
        public OrderService(IUnitOfWork uow, IAuthService authService, IOrderRealtimeService orderRealtimeService, IBranchResolverService branchResolver)
        {
            _branchResolver = branchResolver;
            _authService = authService;
            _realtimeService = orderRealtimeService;
            _uow = uow;
        }

        public async Task<OrderResult> CreateOrderAsync(int branchId, string name, string? phone, bool isTakeAway, List<OrderItem> orderItems)
        {

            var targetBranchId = await _branchResolver.ResolveBranchIdAsync(branchId);

            var branch = await _uow.Branches.GetByIdAsync(targetBranchId);
            if (branch == null)
                return OrderResult.Failed("Branch not found");

            if (orderItems == null || !orderItems.Any())
                return OrderResult.Failed("No items");

            var totalAmount = orderItems.Sum(x => x.Price * x.Quantity);

            var customerType = string.IsNullOrWhiteSpace(phone) ? CustomerType.Guest : CustomerType.Registered;
            var customer = new Customer
            {
                BranchId = branch.BranchId,
                Name = name,
                Phone = phone,
                Type = customerType,
                LoyaltyPoints = 0,
                Tier = LoyaltyTierType.Bronze
            };

            // Tạo Order
            var order = new Order
            {
                
                BranchId = branch.BranchId,
                Customer = customer,
                CurrentStatus = OrderStatus.Pending,
                IsTakeAway = isTakeAway,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = PaymentMethod.Cash,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };


            _uow.Orders.Add(order);
            await _uow.SaveChangesAsync();


            await _realtimeService.BroadcastNewOrderAsync(branchId, order);

            return OrderResult.Success(order);
        }

        public async Task<(IEnumerable<Order> Orders, int BranchId)> GetOrdersByBranchAsync(OrderStatus? status = null)
        {
            var user = await _authService.GetCurrentUserAsync();
            var branchId = user!.BranchId!.Value;

            var orders = await _uow.Orders.GetOrdersByBranchAsync(branchId);
            if (status.HasValue)
                orders = orders.Where(o => o.CurrentStatus == status.Value);

            return (orders.OrderByDescending(o => o.OrderDate), branchId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int staffId)
        {
            var order = await _uow.Orders.GetOrderWithItemsAsync(orderId);
            if (order == null) return false;

            if (!OrderRules.CanChangeStatus(order.CurrentStatus, newStatus))
                throw new InvalidOperationException($"Cannot change status");

            order.CurrentStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;


            if (newStatus == OrderStatus.Completed)
            {
                order.PaymentStatus = PaymentStatus.Completed;
            }

            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();

            await _realtimeService.BroadcastOrderStatusAsync(orderId, newStatus.ToString(), order.BranchId);

            return true;
        }

    }
}
