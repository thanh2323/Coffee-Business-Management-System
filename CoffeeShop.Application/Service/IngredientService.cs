using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Service
{
    
    public class IngredientService : IIngredientService
    {
        private readonly IUnitOfWork _uow;
        public IngredientService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Ingredient>> GetByBranchAsync(int userId, int branchId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return Enumerable.Empty<Ingredient>();

           
            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return Enumerable.Empty<Ingredient>();
            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId
                        && user.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
                return Enumerable.Empty<Ingredient>();


            return await _uow.Ingredients.GetIngredientsByBranchAsync(branchId);
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
            int userId,
            int branchId,
            string name,
            decimal quantity,
            decimal unitCost,
            string? displayUnit)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return IngredientResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null) return IngredientResult.Failed("Branch not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId
                        && user.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
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
            int userId,
            string ingredientName,
            string name,
            decimal quantity,
            decimal unitCost,
            string? displayUnit)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return IngredientResult.Failed("User not found");

            var ing = await _uow.Ingredients.GetByNameAsync(ingredientName);
            if (ing == null) return IngredientResult.Failed("Ingredient not found");
            var branch = await _uow.Branches.GetByIdAsync(ing.BranchId);
            if (branch == null) return IngredientResult.Failed("Branch not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == ing.BranchId
                        && user.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
                return IngredientResult.Failed("Not authorized");

            if (string.IsNullOrWhiteSpace(name)) return IngredientResult.Failed("Name is required");
            if (quantity < 0) return IngredientResult.Failed("Quantity must be >= 0");
            if (unitCost < 0) return IngredientResult.Failed("Unit cost must be >= 0");
            var (baseUnit, conversionFactorToBase) = InferBaseUnitAndConversion(displayUnit);

            var existsName = (await _uow.Ingredients.GetIngredientsByBranchAsync(ing.BranchId))
                .Any(i => i.Name == name);
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

        public async Task<IngredientResult> DeleteAsync(int userId, string ingredientName)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return IngredientResult.Failed("User not found");

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


