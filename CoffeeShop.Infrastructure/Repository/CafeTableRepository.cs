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

        public async Task<CafeTable?> GetByTableNumberAsync(int tableNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.TableNumber == tableNumber);
        }

        public async Task<IEnumerable<CafeTable>> GetAvailableTablesAsync()
        {
            return await _dbSet.Where(t => t.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<CafeTable>> GetTablesByBranchIdAsync(int branchId)
        {
            return await _dbSet.Where(t => t.BranchId == branchId).ToListAsync();
        }
    }
}


