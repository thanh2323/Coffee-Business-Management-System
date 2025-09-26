<<<<<<< Updated upstream
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class BusinessRepository : BaseRepository<Business>, IBusinessRepository
    {
        public BusinessRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Business?> GetByBusinessNameAsync(string businessName)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == businessName);
        }

        public async Task<IEnumerable<Business>> GetActiveBusinessesAsync()
        {
            return await _dbSet.Where(b => b.IsActive).ToListAsync();
        }

        public async Task<Business?> GetByIdAsync(int businessId)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.BusinessId == businessId);
        }
    }
}

=======
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class BusinessRepository : BaseRepository<Business>, IBusinessRepository
    {
        public BusinessRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Business?> GetByBusinessNameAsync(string businessName)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == businessName);
        }

        public async Task<IEnumerable<Business>> GetActiveBusinessesAsync()
        {
            return await _dbSet.Where(b => b.IsActive).ToListAsync();
        }

        public async Task<Business?> GetByIdAsync(int businessId)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.BusinessId == businessId);
        }
    }
}

>>>>>>> Stashed changes
