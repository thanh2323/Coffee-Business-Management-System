using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IMenuItemService
    {
      //  Task<IEnumerable<MenuItem>> GetByBranchAsync(int? branchId);
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(int? branchId, string? category);
        Task<MenuItemResult> GetByIdAsync(int menuItemId);
        Task<MenuItemResult> CreateAsync(int branchId, string name, decimal price, string? category, bool isAvailable = true);
        Task<MenuItemResult> UpdateAsync( int menuItemId, string name, decimal price, string? category, bool isAvailable, int branchId);
        Task<MenuItemResult> DeleteAsync(int menuItemId);
        Task<MenuItemResult> ToggleAvailabilityAsync(int menuItemId);
        Task<IEnumerable<string>> GetCategoriesAsync(int? branchId);
    }

    public class MenuItemResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public MenuItem? MenuItem { get; set; }
        public int? BranchId { get; init; }
        public IEnumerable<string>? Categories { get; set; }

        public static MenuItemResult Success(MenuItem menuItem, string message = "Success") => 
            new MenuItemResult { IsSuccess = true, MenuItem = menuItem, Message = message };
        
        public static MenuItemResult Success(IEnumerable<string> categories, string message = "Success") => 
            new MenuItemResult { IsSuccess = true, Categories = categories, Message = message };
        
        public static MenuItemResult Failed(string message, int? branchId = null) => 
            new MenuItemResult { IsSuccess = false, Message = message, BranchId =  branchId};
    }
}

