using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItem>> GetByBranchAsync(int userId, int branchId);
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(int userId, int branchId, string? category);
        Task<MenuItemResult> GetByIdAsync(int userId, int menuItemId);
        Task<MenuItemResult> CreateAsync(int userId, int branchId, string name, decimal price, string? category, bool isAvailable = true);
        Task<MenuItemResult> UpdateAsync(int userId, int menuItemId, string name, decimal price, string? category, bool isAvailable);
        Task<MenuItemResult> DeleteAsync(int userId, int menuItemId);
        Task<MenuItemResult> ToggleAvailabilityAsync(int userId, int menuItemId);
        Task<IEnumerable<string>> GetCategoriesAsync(int userId, int branchId);
    }

    public class MenuItemResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public MenuItem? MenuItem { get; set; }
        public IEnumerable<string>? Categories { get; set; }

        public static MenuItemResult Success(MenuItem menuItem, string message = "Success") => 
            new MenuItemResult { IsSuccess = true, MenuItem = menuItem, Message = message };
        
        public static MenuItemResult Success(IEnumerable<string> categories, string message = "Success") => 
            new MenuItemResult { IsSuccess = true, Categories = categories, Message = message };
        
        public static MenuItemResult Failed(string message) => 
            new MenuItemResult { IsSuccess = false, Message = message };
    }
}

