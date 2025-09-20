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

     /*   public async Task<IEnumerable<Ingredient>> GetExpiringSoonAsync(int days)
        {
            // Placeholder: there is no expiry field; keeping method for interface parity
            return await _dbSet.ToListAsync();
        }*/
    }
}


