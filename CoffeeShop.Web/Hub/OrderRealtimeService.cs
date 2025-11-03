using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Web.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace CoffeeShop.Web.Hubs
{
    public class OrderRealtimeService : IOrderRealtimeService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderRealtimeService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastNewOrderAsync(int branchId, Order order)
        {
            await _hubContext.Clients.Group($"branch-{branchId}")
                .SendAsync("NewOrderReceived", new
                {
                    order.OrderId,
                    order.Customer.Name,
                    order.Customer.Phone,
                    order.IsTakeAway,
                    order.PaymentMethod,
                    order.PaymentStatus,
                    order.CurrentStatus,
                    order.TotalAmount
                });
        }


        public async Task BroadcastOrderStatusAsync(int orderId, string newStatus, int branchId)
        {
            var message = new { OrderId = orderId, NewStatus = newStatus };

            await _hubContext.Clients.Group($"order-{orderId}")
                .SendAsync("OrderStatusChanged", message);

            await _hubContext.Clients.Group($"branch-{branchId}")
                .SendAsync("OrderStatusChanged", message);
        }

    }
}
