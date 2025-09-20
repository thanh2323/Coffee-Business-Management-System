using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface IIngredientRepository : IBaseRepository<Ingredient>
    {
        // Ingredient-specific methods can be added here
        Task<Ingredient?> GetByNameAsync(string name);
        Task<IEnumerable<Ingredient>> GetLowStockIngredientsAsync(int threshold);
      //  Task<IEnumerable<Ingredient>> GetExpiringSoonAsync(DateTime days);
    }
}