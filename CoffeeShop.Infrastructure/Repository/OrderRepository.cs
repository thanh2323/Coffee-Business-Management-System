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


        public async Task<IEnumerable<Order>> GetOrdersByBranchAsync(int branchId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Customer)
                .Include(o => o.CafeTable)
                .Where(o => o.BranchId == branchId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        // Order with OrderItems (include related data)
        public async Task<Order?> GetOrderWithItemsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                        .ThenInclude(mi => mi.MenuItemRecipes)
                            .ThenInclude(mr => mr.Ingredient)
                .Include(o => o.Customer)
                .Include(o => o.CafeTable)
                .Include(o => o.Branch)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
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


