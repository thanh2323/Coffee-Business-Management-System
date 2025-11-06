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

        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(i => i.IngredientId == id && !i.IsDeleted);
        }

        // ✅ Sửa lại để lấy theo tên và branchId (tránh lấy nhầm chi nhánh khác)
        public async Task<Ingredient?> GetByNameAsync(string name, int branchId)
        {
            return await _dbSet.FirstOrDefaultAsync(i => i.Name == name && i.BranchId == branchId && !i.IsDeleted);
        }

        public async Task<IEnumerable<Ingredient>> GetLowStockIngredientsAsync(int threshold)
        {
            return await _dbSet.Where(i => i.Quantity <= threshold && !i.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientsByBranchAsync(int branchId)
        {
            return await _dbSet
                .Include(i => i.Branch)
                .Where(i => i.BranchId == branchId && !i.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameInBranchAsync(int branchId, string name, int? excludeId = null)
        {
            var query = _context.Ingredients
                .Where(i => i.BranchId == branchId && !i.IsDeleted && i.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.IngredientId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

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
