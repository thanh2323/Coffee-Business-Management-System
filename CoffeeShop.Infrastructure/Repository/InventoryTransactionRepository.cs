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
    public class InventoryTransactionRepository : BaseRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

   

        public async Task<IEnumerable<InventoryTransaction>> GetByIngredientIdAsync(int ingredientId)
        {
            return await _dbSet.Where(tx => tx.IngredientId == ingredientId).ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByTransactionTypeAsync(TransactionType transactionType)
        {
            return await _dbSet.Where(tx => tx.Type == transactionType).ToListAsync();
        }

  
        public async Task<decimal> GetTotalQuantityChangeAsync(int ingredientId)
        {
            return await _dbSet.Where(tx => tx.IngredientId == ingredientId)
                .SumAsync(tx => tx.QuantityChange);
        }
    }
}


