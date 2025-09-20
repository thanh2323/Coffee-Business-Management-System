using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet.Where(o => o.CurrentStatus == status).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByPaymentStatusAsync(PaymentStatus paymentStatus)
        {
            return await _dbSet.Where(o => o.PaymentStatus == paymentStatus).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            return await _dbSet.Where(o => o.PaymentMethod == paymentMethod).ToListAsync();
        }

        // Order with OrderItems (include related data)
        public async Task<Order?> GetOrderWithItemsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Customer)
                .Include(o => o.CafeTable)
                .Include(o => o.Branch)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersWithItemsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Customer)
                .Include(o => o.CafeTable)
                .Include(o => o.Branch)
                .Include(o => o.User)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        // OrderItem CRUD methods (since OrderItem is part of Order aggregate)
        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId)
        {
            return await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
        }

        public void UpdateOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
        }

        public void DeleteOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Remove(orderItem);
        }

        public void DeleteOrderItemsByOrderId(int orderId)
        {
            var orderItems = _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToList();

            if (orderItems.Any())
            {
                _context.OrderItems.RemoveRange(orderItems);
            }
        }
    }
}


