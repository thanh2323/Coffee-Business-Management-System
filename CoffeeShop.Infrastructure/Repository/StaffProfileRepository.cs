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
    public class StaffProfileRepository : BaseRepository<StaffProfile>, IStaffProfileRepository
    {
        public StaffProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<StaffProfile?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<IEnumerable<StaffProfile>> GetByBranchIdAsync(int branchId)
        {
            return await _dbSet.Include(s => s.User)
                .Where(s => s.User.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StaffProfile>> GetByRoleAsync(StaffRole role)
        {
            return await _dbSet.Include(s => s.User)
                .Where(s => s.Position == role)
                .ToListAsync();
        }

        public async Task<IEnumerable<StaffProfile>> GetActiveStaffAsync()
        {
            return await _dbSet.Include(s => s.User)
                .Where(s => !s.User.IsDeleted) // Active users only
                .ToListAsync();
        }

        public async Task<IEnumerable<StaffProfile>> GetStaffByBusinessIdAsync(int businessId)
        {
            return await _dbSet.Include(s => s.User)
                .Where(s => s.User.BusinessId == businessId)
                .ToListAsync();
        }
    }
}

