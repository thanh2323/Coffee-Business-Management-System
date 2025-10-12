using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IIngredientService
    {
        Task<IEnumerable<Ingredient>> GetByBranchAsync(int? branchId);
        Task<IngredientResult> CreateAsync(int branchId, string name, decimal quantity, decimal unitCost, string? displayUnit);
        Task<IngredientResult> UpdateAsync(int branchId, string ingredientName, string name, decimal quantity, decimal unitCost, string? displayUnit);
        Task<IngredientResult> DeleteAsync(string ingredientName);
    }

    public class IngredientResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Ingredient? Ingredient { get; set; }

        public static IngredientResult Success(Ingredient ingredient, string message = "Success")
            => new IngredientResult { IsSuccess = true, Ingredient = ingredient, Message = message };
        public static IngredientResult Failed(string message)
            => new IngredientResult { IsSuccess = false, Message = message };
    }
}


