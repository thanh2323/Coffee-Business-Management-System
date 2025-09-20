using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class IngredientRepository : BaseRepository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Ingredient?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(i => i.Name == name);
        }

        public async Task<IEnumerable<Ingredient>> GetLowStockIngredientsAsync(int threshold)
        {
            return await _dbSet.Where(i => i.Quantity <= threshold).ToListAsync();
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientsByBranchAsync(int branchId)
        {
            return await _dbSet
                .Include(i => i.Branch)
                .Where(i => i.BranchId == branchId)
                .ToListAsync();
        }

        // InventoryTransaction CRUD methods (since InventoryTransaction is part of Ingredient aggregate)
        public async Task<IEnumerable<InventoryTransaction>> GetInventoryTransactionsByIngredientIdAsync(int ingredientId)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Ingredient)
                .Where(t => t.IngredientId == ingredientId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<InventoryTransaction?> GetInventoryTransactionByIdAsync(int transactionId)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Ingredient)
                .FirstOrDefaultAsync(t => t.InventoryTxnId == transactionId);
        }

        public void AddInventoryTransaction(InventoryTransaction transaction)
        {
            _context.InventoryTransactions.Add(transaction);
        }

        public void DeleteInventoryTransaction(InventoryTransaction transaction)
        {
            _context.InventoryTransactions.Remove(transaction);
        }
    }
}


