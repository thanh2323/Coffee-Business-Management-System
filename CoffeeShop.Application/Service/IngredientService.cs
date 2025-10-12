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
            if (branchId.HasValue)
                if (branchId.Value <= 0)
                    throw new ArgumentException("Invalid branch ID.");
                else
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
            // Simple heuristics: support common packaging shortcuts
            // Examples: "1l", "500ml", "5kg", "250g", "10pcs"
            // If token starts with number then unit, parse; otherwise, fallback 1
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

            // Infer base + convert
            switch (unit)
            {
                case "ml":
                    return (BaseUnit.ml, number);
                case "l":
                    return (BaseUnit.ml, number * 1000m);
                case "g":
                    return (BaseUnit.g, number);
                case "kg":
                    return (BaseUnit.g, number * 1000m);
                case "pcs":
                case "pc":
                    return (BaseUnit.pcs, number);
                default:
                    return (BaseUnit.pcs, number);
            }
        }

        public async Task<IngredientResult> CreateAsync(
            int branchId,
            string name,
            decimal quantity,
            decimal unitCost,
            string? displayUnit)
        {
            var user = await _authService.GetCurrentUserAsync();
           
            if (user == null) 
                return IngredientResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null) 
                return IngredientResult.Failed("Branch not found");
            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return IngredientResult.Failed("Not authorized");

            // Validation
            if (string.IsNullOrWhiteSpace(name)) return IngredientResult.Failed("Name is required");
            if (quantity < 0) return IngredientResult.Failed("Quantity must be >= 0");
            if (unitCost < 0) return IngredientResult.Failed("Unit cost must be >= 0");
            var (baseUnit, conversionFactorToBase) = InferBaseUnitAndConversion(displayUnit);

            // Unique per branch
            var existingInBranch = (await _uow.Ingredients.GetIngredientsByBranchAsync(branchId))
                .Any(i => i.Name == name && i.BranchId == branchId);
            if (existingInBranch) return IngredientResult.Failed("Ingredient name already exists in this branch");

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

        public async Task<IngredientResult> UpdateAsync(
            int branchId,
            string ingredientName,
            string name,
            decimal quantity,
            decimal unitCost,
            string? displayUnit)
        {

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return IngredientResult.Failed("User not found");
         

            var ing = await _uow.Ingredients.GetByNameAsync(ingredientName);
            if (ing == null) return IngredientResult.Failed("Ingredient not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null) return IngredientResult.Failed("Branch not found");

            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return IngredientResult.Failed("Not authorized");
           

            if (string.IsNullOrWhiteSpace(name)) return IngredientResult.Failed("Name is required");
            if (quantity < 0) return IngredientResult.Failed("Quantity must be >= 0");
            if (unitCost < 0) return IngredientResult.Failed("Unit cost must be >= 0");
            var (baseUnit, conversionFactorToBase) = InferBaseUnitAndConversion(displayUnit);

            var existsName = await _uow.Ingredients.ExistsByNameInBranchAsync(branchId, name, ing.IngredientId);
            if (existsName) return IngredientResult.Failed("Ingredient name already exists in this branch");

            ing.BranchId = branchId;
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

            var ing = await _uow.Ingredients.GetByNameAsync(ingredientName);
            if (ing == null) return IngredientResult.Failed("Ingredient not found");

            var branch = await _uow.Branches.GetByIdAsync(ing.BranchId);
            if (branch == null) return IngredientResult.Failed("Branch not found");

            bool isOwner = user.Role == UserRole.Owner && user.BusinessId == branch.BusinessId;
            bool isManager = user.Role == UserRole.Staff && user.BranchId == ing.BranchId;
            if (!isOwner && !isManager) return IngredientResult.Failed("Not authorized");

            _uow.Ingredients.SoftDelete(ing);
            await _uow.SaveChangesAsync();
            return IngredientResult.Success(ing, "Ingredient deleted");
        }
    }
}


