using System;
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
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == branchName);
        }

        public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
        {
            // Global query filter excludes deleted entities
            return await _dbSet.ToListAsync();
        }
    }
}
