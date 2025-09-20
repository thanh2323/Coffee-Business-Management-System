using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _dbSet.Where(oi => oi.OrderId == orderId).ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByMenuItemIdAsync(int menuItemId)
        {
            return await _dbSet.Where(oi => oi.MenuItemId == menuItemId).ToListAsync();
        }
    }
}


