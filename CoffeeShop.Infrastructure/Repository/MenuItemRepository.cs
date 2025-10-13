using System.Reflection.Metadata.Ecma335;
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Infrastructure.Repository
{
    public class MenuItemRepository : BaseRepository<MenuItem>, IMenuItemRepository
    {
        public MenuItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MenuItem>> GetByBranchIdAsync(int branchId)
        {
            return await _context.MenuItems
                .Where(m => m.BranchId == branchId && !m.IsDeleted)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetByBranchAndCategoryAsync(int branchId, string? category)
        {
            var query = _context.MenuItems
                .Where(m => m.BranchId == branchId && !m.IsDeleted);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category == category);
            }

            return await query
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<MenuItem?> GetByIdWithRecipesAsync(int menuItemId)
        {
            return await _context.MenuItems
                .Include(m => m.MenuItemRecipes)
                    .ThenInclude(r => r.Ingredient)
                .Include(m => m.Branch)
                .FirstOrDefaultAsync(m => m.MenuItemId == menuItemId && !m.IsDeleted);
        }

        public async Task<bool> ExistsByNameInBranchAsync(int branchId, string name, int? excludeId = null)
        {
            var query = _context.MenuItems
                .Where(m => m.BranchId == branchId && !m.IsDeleted && m.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.MenuItemId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.MenuItemId == id && !m.IsDeleted );
                   
        }
    }
}