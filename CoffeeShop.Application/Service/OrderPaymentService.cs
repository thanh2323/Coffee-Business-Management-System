using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Service
{
    public class OrderPaymentService : IOrderPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITempOrderRepository _tempOrderRepo;
        private readonly IOrderRealtimeService _orderRealtimeService;
        private readonly IInventoryService _inventoryService;

        public OrderPaymentService(IUnitOfWork uow, ITempOrderRepository tempOrderRepo , IOrderRealtimeService orderRealtimeService, IInventoryService inventoryService)
        {
            _orderRealtimeService = orderRealtimeService;
            _uow = uow;
            _tempOrderRepo = tempOrderRepo;
            _inventoryService = inventoryService;
        }
        public async Task<bool> ConvertTempOrderToRealOrderAsync(string tempOrderId, PaymentGateway gateway)
        {
            // 1. Get TempOrder from Redis
            var tempOrder = await _tempOrderRepo.GetAsync(tempOrderId);
            if (tempOrder == null)
                return false;

            await _uow.BeginTransactionAsync();
            try
            {
                // 2. Find or create Customer
                var customer = await _uow.Customers.GetByNameOrPhoneAsync(tempOrder.CustomerName, tempOrder.CustomerPhone);


                if (customer == null)
                {
                    var customerType = string.IsNullOrWhiteSpace(tempOrder.CustomerPhone) ? CustomerType.Guest : CustomerType.Registered;
                    customer = new Customer
                    {
                        Name = tempOrder.CustomerName,
                        Phone = tempOrder.CustomerPhone,
                        BranchId = tempOrder.BranchId,
                        Type = customerType,
                        LoyaltyPoints = 0,
                        Tier = LoyaltyTierType.Bronze
                    };
                    _uow.Customers.Add(customer);
                    await _uow.SaveChangesAsync();
                }

                // 3. Create Order
                var order = new Order
                {
                    CustomerId = customer.CustomerId,
                    BranchId = tempOrder.BranchId,
                    IsTakeAway = tempOrder.IsTakeAway,
                    OrderDate = DateTime.UtcNow,
                    PaymentMethod = gateway switch
                    {
                        PaymentGateway.VNPay => PaymentMethod.BankTransfer,
                        PaymentGateway.MoMo => PaymentMethod.BankTransfer,
                        _ => PaymentMethod.Cash
                    },
                    PaymentStatus = PaymentStatus.Completed,
                    PaidAt = DateTime.UtcNow,
                    CurrentStatus = OrderStatus.Confirmed,
                    TotalAmount = tempOrder.TotalAmount
                };

                _uow.Orders.Add(order);
                await _uow.SaveChangesAsync();

                // 4. Add OrderItems
                foreach (var item in tempOrder.Items)
                {
                    _uow.Orders.AddOrderItem(new OrderItem
                    {
                        OrderId = order.OrderId,
                        MenuItemId = item.MenuItemId,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice
                    });
                }

                await _uow.SaveChangesAsync();
                await _inventoryService.DeductInventoryFromOrderAsync(order);

                // 5. Remove TempOrder from Redis
                await _tempOrderRepo.DeleteAsync(tempOrderId);

                await _uow.CommitTransactionAsync();
                await _orderRealtimeService.BroadcastNewOrderAsync(order.BranchId,order);
                return true;
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                return false;
            }
        }

    }
}
