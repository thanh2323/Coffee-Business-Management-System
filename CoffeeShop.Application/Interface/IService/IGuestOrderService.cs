using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IGuestOrderService
    {
        Task<GuestOrderResult> GetMenuForTableAsync(int tableId, int branchId);
        Task<GuestOrderResult> GetCartAsync(string sessionId);
        Task<GuestOrderResult> AddToCartAsync(string sessionId, int menuItemId, int quantity, int branchId, int? tableId);
        Task<GuestOrderResult> UpdateCartAsync(string sessionId, int menuItemId, int quantity);
        Task<GuestOrderResult> RemoveFromCartAsync(string sessionId, int menuItemId);
        Task<GuestOrderResult> ClearCartAsync(string sessionId);
        Task<GuestOrderResult> GetCartDetailsAsync(string sessionId);
        Task<GuestOrderResult> CreateTempOrderAsync(string customerName, string? customerPhone, int tableId, int branchId, string sessionId);
    }

    public class GuestOrderResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public CafeTable? Table { get; set; }
        public Branch? Branch { get; set; }
        public IEnumerable<MenuItem>? MenuItems { get; set; }
        public List<CartItemWithDetails>? CartItems { get; set; }
        public decimal? TotalAmount { get; set; }
        public TempOrder? TempOrder { get; set; }

        public static GuestOrderResult Success(CafeTable table, Branch branch, IEnumerable<MenuItem> menuItems)
            => new GuestOrderResult { IsSuccess = true, Table = table, Branch = branch, MenuItems = menuItems };

        //public static GuestOrderResult Success(CafeTable table, Branch branch, List<CartItemWithDetails> cartItems, decimal totalAmount)
        //    => new GuestOrderResult { IsSuccess = true, Table = table, Branch = branch, CartItems = cartItems, TotalAmount = totalAmount };

        public static GuestOrderResult Success(TempOrder tempOrder)
            => new GuestOrderResult { IsSuccess = true, TempOrder = tempOrder };

        public static GuestOrderResult Success(List<CartItemWithDetails> cartItems, decimal totalAmount)
            => new GuestOrderResult { IsSuccess = true, CartItems = cartItems, TotalAmount = totalAmount };

        public static GuestOrderResult Failed(string message)
            => new GuestOrderResult { IsSuccess = false, Message = message };
    }

    public class CartItem
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemWithDetails : CartItem
    {
        public string MenuItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}


