using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Rules;

namespace CoffeeShop.Application.Service
{
    public class IngredientService : IIngredientService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _authService;

        public IngredientService(IUnitOfWork uow, IAuthService authService)
        {
            _uow = uow;
            _authService = authService;
        }

        public async Task<IEnumerable<Ingredient>> GetByBranchAsync(int? branchId = null)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("User not found");

            int targetBranchId;
            if (branchId.HasValue && branchId.Value > 0)
                targetBranchId = branchId.Value;
            else if (user.BranchId.HasValue)
                targetBranchId = user.BranchId.Value;
            else
                throw new Exception("No branch specified");

            return await _uow.Ingredients.GetIngredientsByBranchAsync(targetBranchId);
        }

        private static (BaseUnit baseUnit, decimal baseUnitsPerPackage) InferBaseUnitAndConversion(string? displayUnit)
        {
            if (string.IsNullOrWhiteSpace(displayUnit)) return (BaseUnit.pcs, 1m);
            var token = displayUnit.Trim().ToLowerInvariant();
            decimal number = 1m;
            string unit = string.Empty;
            for (int i = 0; i < token.Length; i++)
            {
                if (!char.IsDigit(token[i]) && token[i] != '.')
                {
                    number = decimal.TryParse(token.Substring(0, i), out var n) ? n : 1m;
                    unit = token.Substring(i);
                    break;
                }
            }

            if (string.IsNullOrEmpty(unit)) return (BaseUnit.pcs, number);
            return unit switch
            {
                "ml" => (BaseUnit.ml, number),
                "l" => (BaseUnit.ml, number * 1000m),
                "g" => (BaseUnit.g, number),
                "kg" => (BaseUnit.g, number * 1000m),
                "pcs" or "pc" => (BaseUnit.pcs, number),
                _ => (BaseUnit.pcs, number)
            };
        }

        public async Task<IngredientResult> CreateAsync(int branchId, string name, decimal quantity, decimal unitCost, string? displayUnit)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return IngredientResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return IngredientResult.Failed("Branch not found");

            if (!_authService.CanManageBranch(user, branch))
                return IngredientResult.Failed("Not authorized");

            if (string.IsNullOrWhiteSpace(name)) return IngredientResult.Failed("Name is required");
            if (quantity < 0) return IngredientResult.Failed("Quantity must be >= 0");
            if (unitCost < 0) return IngredientResult.Failed("Unit cost must be >= 0");

            var (baseUnit, conversionFactorToBase) = InferBaseUnitAndConversion(displayUnit);
            var exists = await _uow.Ingredients.ExistsByNameInBranchAsync(branchId, name);
            if (exists) return IngredientResult.Failed("Ingredient name already exists in this branch");

            var entity = new Ingredient
            {
                BranchId = branchId,
                Name = name.Trim(),
                Quantity = quantity,
                UnitCost = unitCost,
                DisplayUnit = displayUnit,
                ConversionFactorToBase = conversionFactorToBase,
                BaseUnit = baseUnit
            };

            _uow.Ingredients.Add(entity);
            await _uow.SaveChangesAsync();
            return IngredientResult.Success(entity, "Ingredient created");
        }

        public async Task<IngredientResult> UpdateAsync(int branchId, string ingredientName, string name, decimal quantity, decimal unitCost, string? displayUnit)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return IngredientResult.Failed("User not found");

            // ✅ Giờ đây chỉ lấy nguyên liệu trong đúng chi nhánh
            var ing = await _uow.Ingredients.GetByNameAsync(ingredientName, branchId);
            if (ing == null)
                return IngredientResult.Failed("Ingredient not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return IngredientResult.Failed("Branch not found");

            if (!_authService.CanManageBranch(user, branch))
                return IngredientResult.Failed("Not authorized");

            if (string.IsNullOrWhiteSpace(name)) return IngredientResult.Failed("Name is required");
            if (quantity < 0) return IngredientResult.Failed("Quantity must be >= 0");
            if (unitCost < 0) return IngredientResult.Failed("Unit cost must be >= 0");

            var (baseUnit, conversionFactorToBase) = InferBaseUnitAndConversion(displayUnit);

            // ✅ Sửa lại để bỏ qua chính nó khi kiểm tra trùng tên
            var existsName = await _uow.Ingredients.ExistsByNameInBranchAsync(branchId, name, ing.IngredientId);
            if (existsName) return IngredientResult.Failed("Ingredient name already exists in this branch");

            ing.Name = name.Trim();
            ing.Quantity = quantity;
            ing.UnitCost = unitCost;
            ing.DisplayUnit = displayUnit;
            ing.ConversionFactorToBase = conversionFactorToBase;
            ing.BaseUnit = baseUnit;

            _uow.Ingredients.Update(ing);
            await _uow.SaveChangesAsync();
            return IngredientResult.Success(ing, "Ingredient updated");
        }

        public async Task<IngredientResult> DeleteAsync(string ingredientName)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return IngredientResult.Failed("User not found");

            var ing = await _uow.Ingredients.GetByNameAsync(ingredientName, user.BranchId ?? 0);
            if (ing == null)
                return IngredientResult.Failed("Ingredient not found");

            var branch = await _uow.Branches.GetByIdAsync(ing.BranchId);
            if (branch == null)
                return IngredientResult.Failed("Branch not found");

            if (!_authService.CanManageBranch(user, branch))
                return IngredientResult.Failed("Not authorized");

            _uow.Ingredients.SoftDelete(ing);
            await _uow.SaveChangesAsync();
            return IngredientResult.Success(ing, "Ingredient deleted");
        }
    }
}
