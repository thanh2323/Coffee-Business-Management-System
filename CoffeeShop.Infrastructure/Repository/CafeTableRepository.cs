using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class CafeTableRepository : BaseRepository<CafeTable>, ICafeTableRepository
    {
        public CafeTableRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CafeTable?> GetByBranchAndNumberAsync(int branchId, int tableNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.BranchId == branchId && t.TableNumber == tableNumber);
        }

        public async Task<IEnumerable<CafeTable>> GetByBranchAsync(int branchId)
        {
            return await _dbSet.Where(t => t.BranchId == branchId).ToListAsync();
        }

        public async Task<IEnumerable<CafeTable>> GetActiveByBranchAsync(int branchId)
        {
            return await _dbSet.Where(t => t.BranchId == branchId && t.IsActive).ToListAsync();
        }

        public async Task<CafeTable?> GetByIdAsync(int tableId)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.TableId == tableId);
        }
    }
}




