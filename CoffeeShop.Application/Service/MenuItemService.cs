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

        public MenuItemService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<MenuItem>> GetByBranchAsync(int userId, int branchId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<MenuItem>();

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return Enumerable.Empty<MenuItem>();

            // Authorization: Owner of business or Staff of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isStaffManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId;

            if (!isOwnerManaging && !isStaffManaging)
                return Enumerable.Empty<MenuItem>();

            return await _uow.MenuItems.GetByBranchIdAsync(branchId);
        }

        public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(int userId, int branchId, string? category)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<MenuItem>();

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return Enumerable.Empty<MenuItem>();

            // Authorization: Owner of business or Staff of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isStaffManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId;

            if (!isOwnerManaging && !isStaffManaging)
                return Enumerable.Empty<MenuItem>();

            return await _uow.MenuItems.GetByBranchAndCategoryAsync(branchId, category);
        }

        public async Task<MenuItemResult> GetByIdAsync(int userId, int menuItemId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            // Authorization: Owner of business or Staff of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && menuItem.Branch.BusinessId == user.BusinessId.Value;

            var isStaffManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == menuItem.BranchId;

            if (!isOwnerManaging && !isStaffManaging)
                return MenuItemResult.Failed("Not authorized to access this menu item");

            return MenuItemResult.Success(menuItem);
        }

        public async Task<MenuItemResult> CreateAsync(int userId, int branchId, string name, decimal price, string? category, bool isAvailable = true)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
                return MenuItemResult.Failed("Name is required");

            if (!MenuItemRules.IsValidPrice(price))
                return MenuItemResult.Failed($"Price must be between {DomainRules.MIN_PRICE:N0} and {DomainRules.MAX_PRICE:N0} VND");

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return MenuItemResult.Failed("Branch not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId
                        && user.StaffProfile?.Position == StaffRole.Manager;

            if (!isOwnerManaging && !isManagerManaging)
                return MenuItemResult.Failed("Not authorized to manage this branch");

            // Business active check only for owner path
            if (isOwnerManaging)
            {
                var business = await _uow.Businesses.GetByIdAsync(branch.BusinessId);
                if (business == null)
                    return MenuItemResult.Failed("Business not found");
                if (!business.IsActive)
                    return MenuItemResult.Failed("Business is not active");
            }

            // Check if name already exists in this branch
            var exists = await _uow.MenuItems.ExistsByNameInBranchAsync(branchId, name);
            if (exists)
                return MenuItemResult.Failed("Menu item name already exists in this branch");

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

        public async Task<MenuItemResult> UpdateAsync(int userId, int menuItemId, string name, decimal price, string? category, bool isAvailable)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
                return MenuItemResult.Failed("Name is required");

            if (!MenuItemRules.IsValidPrice(price))
                return MenuItemResult.Failed($"Price must be between {DomainRules.MIN_PRICE:N0} and {DomainRules.MAX_PRICE:N0} VND");

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && menuItem.Branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == menuItem.BranchId
                        && user.StaffProfile?.Position == StaffRole.Manager;

            if (!isOwnerManaging && !isManagerManaging)
                return MenuItemResult.Failed("Not authorized to manage this menu item");

            // Check if name already exists in this branch (excluding current item)
            var exists = await _uow.MenuItems.ExistsByNameInBranchAsync(menuItem.BranchId, name, menuItemId);
            if (exists)
                return MenuItemResult.Failed("Menu item name already exists in this branch");

            menuItem.Name = name.Trim();
            menuItem.Price = price;
            menuItem.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            menuItem.IsAvailable = isAvailable;
            menuItem.MarkAsUpdated();

            _uow.MenuItems.Update(menuItem);
            await _uow.SaveChangesAsync();

            return MenuItemResult.Success(menuItem, "Menu item updated successfully");
        }

        public async Task<MenuItemResult> DeleteAsync(int userId, int menuItemId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && menuItem.Branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == menuItem.BranchId
                        && user.StaffProfile?.Position == StaffRole.Manager;

            if (!isOwnerManaging && !isManagerManaging)
                return MenuItemResult.Failed("Not authorized to manage this menu item");

            _uow.MenuItems.SoftDelete(menuItem);
            await _uow.SaveChangesAsync();

            return MenuItemResult.Success(menuItem, "Menu item deleted successfully");
        }

        public async Task<MenuItemResult> ToggleAvailabilityAsync(int userId, int menuItemId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return MenuItemResult.Failed("User not found");

            var menuItem = await _uow.MenuItems.GetByIdWithRecipesAsync(menuItemId);
            if (menuItem == null)
                return MenuItemResult.Failed("Menu item not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && menuItem.Branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == menuItem.BranchId
                        && user.StaffProfile?.Position == StaffRole.Manager;

            if (!isOwnerManaging && !isManagerManaging)
                return MenuItemResult.Failed("Not authorized to manage this menu item");

            menuItem.IsAvailable = !menuItem.IsAvailable;
            menuItem.MarkAsUpdated();

            _uow.MenuItems.Update(menuItem);
            await _uow.SaveChangesAsync();

            var status = menuItem.IsAvailable ? "available" : "unavailable";
            return MenuItemResult.Success(menuItem, $"Menu item marked as {status}");
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync(int userId, int branchId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<string>();

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return Enumerable.Empty<string>();

            // Authorization: Owner of business or Staff of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isStaffManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId;

            if (!isOwnerManaging && !isStaffManaging)
                return Enumerable.Empty<string>();

            var menuItems = await _uow.MenuItems.GetByBranchIdAsync(branchId);
            return menuItems
                .Where(m => !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
}

