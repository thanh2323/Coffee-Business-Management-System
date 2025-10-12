using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Rules;

namespace CoffeeShop.Application.Service
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _authService;
        public MenuItemService(IUnitOfWork uow, IAuthService authService)
        {
            _uow = uow;
            _authService = authService;
        }


        private async Task<MenuItemResult> ValidateMenuItemUpdateAsync(string name, decimal price, int branchId, int? currentItemId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return MenuItemResult.Failed("Name is required.");

            if (!MenuItemRules.IsValidPrice(price))
                return MenuItemResult.Failed("Invalid price.");

            // Check duplicate (ignore current item)
            var exists = await _uow.MenuItems.ExistsByNameInBranchAsync(branchId, name, currentItemId);
            if (exists)
                return MenuItemResult.Failed("A menu item with this name already exists in this branch.", branchId);

            return MenuItemResult.Success(new List<string>(),"Validation passed");
        }
        public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(int? branchId, string? category)
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


            return await _uow.MenuItems.GetByBranchAndCategoryAsync(targetBranchId, category);
        }

        public async Task<MenuItemResult> GetByIdAsync(int menuItemId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            var userCanManage = _authService.CanManageBranch(user, menuItem.Branch);
            if (!userCanManage)
                return MenuItemResult.Failed("Not authorized to access this menu item");

            return MenuItemResult.Success(menuItem);
        }

        public async Task<MenuItemResult> CreateAsync(int branchId, string name, decimal price, string? category, bool isAvailable = true)
        {


            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return MenuItemResult.Failed("Branch not found");

            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return MenuItemResult.Failed("Not authorized to access");

            var validationResult = await ValidateMenuItemUpdateAsync(name, price, branchId);
            if (!validationResult.IsSuccess)
                return MenuItemResult.Failed("A menu item with this name already exists in this branch.", branchId);

            var menuItem = new MenuItem
            {
                BranchId = branchId,
                Name = name.Trim(),
                Price = price,
                Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
                IsAvailable = isAvailable,
                CreatedAt = DateTime.UtcNow
            };

            _uow.MenuItems.Add(menuItem);
            await _uow.SaveChangesAsync();

            return MenuItemResult.Success(menuItem, "Menu item created successfully");
        }

        public async Task<MenuItemResult> UpdateAsync(int menuItemId, string name, decimal price, string? category, bool isAvailable, int branchId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return MenuItemResult.Failed("User not found");
            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return MenuItemResult.Failed("Branch not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return MenuItemResult.Failed("Not authorized to manage this menu item");

            var validationResult = await ValidateMenuItemUpdateAsync(name, price, branchId, menuItemId);
            if (!validationResult.IsSuccess)
                return MenuItemResult.Failed("A menu item with this name already exists in this branch.", branchId);

            menuItem.Name = name.Trim();
            menuItem.Price = price;
            menuItem.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            menuItem.IsAvailable = isAvailable;
            menuItem.MarkAsUpdated();

            _uow.MenuItems.Update(menuItem);
            await _uow.SaveChangesAsync();

            return MenuItemResult.Success(menuItem, "Menu item updated successfully");
        }

        public async Task<MenuItemResult> DeleteAsync(int menuItemId)
        {

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return MenuItemResult.Failed("User not found");
            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            var userCanManage = _authService.CanManageBranch(user, menuItem.Branch);
            if (!userCanManage)
                return MenuItemResult.Failed("Not authorized to manage this menu item");


            _uow.MenuItems.SoftDelete(menuItem);
            await _uow.SaveChangesAsync();

            return MenuItemResult.Success(menuItem, "Menu item deleted successfully");
        }

        public async Task<MenuItemResult> ToggleAvailabilityAsync(int menuItemId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            var userCanManage = _authService.CanManageBranch(user, menuItem.Branch);
            if (!userCanManage)
                return MenuItemResult.Failed("Not authorized to manage this menu item");


            menuItem.IsAvailable = !menuItem.IsAvailable;
            menuItem.MarkAsUpdated();

            _uow.MenuItems.Update(menuItem);
            await _uow.SaveChangesAsync();

            var status = menuItem.IsAvailable ? "available" : "unavailable";
            return MenuItemResult.Success(menuItem, $"Menu item marked as {status}");
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync(int? branchId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("User not found");

            int targetBranchId;
            if (branchId.HasValue)
                targetBranchId = branchId.Value;
            else if (user.BranchId.HasValue)
                targetBranchId = user.BranchId.Value;
            else
                throw new Exception("No branch specified");



            var menuItems = await _uow.MenuItems.GetByBranchIdAsync(targetBranchId);
            return menuItems
                .Where(m => !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
}

