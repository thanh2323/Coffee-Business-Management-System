using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class BranchRepository : BaseRepository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Branch?> GetByNameAsync(string branchName)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == branchName && !b.IsDeleted);
        }

        public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
        {
            // Global query filter excludes deleted entities
            return await _dbSet.Where(b => !b.IsDeleted).ToListAsync();
        }

        // ✅ Cách 2: Cả hai phương thức cùng trỏ đến 1 logic
        public async Task<IEnumerable<Branch>> GetByBusinessAsync(int businessId)
        {
            return await _dbSet
                .Where(b => b.BusinessId == businessId && !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Branch>> GetByBusinessIdAsync(int businessId)
        {
            // Gọi lại hàm trên để tránh trùng logic
            return await GetByBusinessAsync(businessId);
        }

        public async Task<bool> ExistsByNameAsync(int businessId, string name)
        {
            return await _dbSet.AnyAsync(b => b.BusinessId == businessId && b.Name == name && !b.IsDeleted);
        }

        public async Task<Branch?> GetByIdAsync(int branchId)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.BranchId == branchId && !b.IsDeleted);
        }
    }
}
