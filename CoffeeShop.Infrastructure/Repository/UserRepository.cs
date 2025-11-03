using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Infrastructure.Data;
using CoffeeShop.Infrastructure.Extention;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                        .Include(u => u.StaffProfile)
                        .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetStaffByBranchAsync(int branchId)
        {
            return await _dbSet.Include(u => u.StaffProfile)
                               .Where(u => u.BranchId == branchId)
                               .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            var user = await _dbSet
                             .Include(u => u.StaffProfile)
                             .Include(u => u.Business)
                             .Include(u => u.Branch)
                             .FirstOrDefaultAsync(u => u.UserId == id);

           
            return user;
        }
    }
}


