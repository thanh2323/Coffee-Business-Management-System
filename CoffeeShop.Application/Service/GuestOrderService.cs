using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Service
{
    public class GuestOrderService : IGuestOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITempOrderRepository _tempOrderRepository;
        public GuestOrderService(IUnitOfWork uow, ITempOrderRepository tempOrderRepository)
        {
            _uow = uow;
            _tempOrderRepository = tempOrderRepository;
        }

        public async Task<GuestOrderResult> GetMenuForTableAsync(int tableId, int branchId)
        {
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            var branch = await _uow.Branches.GetByIdAsync(branchId);
            var menuItems = await _uow.MenuItems.GetByBranchIdAsync(branchId);

            if (table == null || branch == null)
                return GuestOrderResult.Failed("Table or branch not found");

            return GuestOrderResult.Success(table, branch, menuItems);
        }

 
        public async Task<GuestOrderResult> AddToCartAsync(string sessionId, int menuItemId, int quantity, int branchId, int? tableId)
        {

            var cart = await _tempOrderRepository.GetAsync(sessionId);

            if (cart == null)
            {
                // Create new cart
                cart = new TempOrder
                {
                    TempOrderId = sessionId,
                    BranchId = branchId,
                    TableId = tableId,
                    CustomerName = "",
                    CustomerPhone = null,
                    Items = new List<TempOrderItem>(),
                    TotalAmount = 0,
                    DiscountAmount = 0,
                    PayableAmount = 0,
                    RedeemPoints = 0,
                    PaymentReference = "",
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Get menu item details
            var menuItem = await _uow.MenuItems.GetByIdAsync(menuItemId);
            if (menuItem == null)
                return GuestOrderResult.Failed("Menu item not found");

            // Check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                cart.Items.Add(new TempOrderItem
                {
                    MenuItemId = menuItemId,
                    MenuItemName = menuItem.Name,
                    Quantity = quantity,
                    UnitPrice = menuItem.Price,
                    TotalPrice = menuItem.Price * quantity
                });
            }

            // Update totals
            cart.TotalAmount = cart.Items.Sum(x => x.TotalPrice);
            cart.PayableAmount = cart.TotalAmount - cart.DiscountAmount;

            // Save cart
            await _tempOrderRepository.SetAsync(cart, TimeSpan.FromHours(2));

            return GuestOrderResult.Success(cart);
        }

        public async Task<GuestOrderResult> UpdateCartAsync(string sessionId, int menuItemId, int quantity)
        {

            var cart = await _tempOrderRepository.GetAsync(sessionId);

            if (cart == null)
                return GuestOrderResult.Failed("Cart not found");

            var item = cart.Items.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (item == null)
                return GuestOrderResult.Failed("Item not found in cart");

            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                item.TotalPrice = item.UnitPrice * quantity;
            }

            // Update totals
            cart.TotalAmount = cart.Items.Sum(x => x.TotalPrice);
            cart.PayableAmount = cart.TotalAmount - cart.DiscountAmount;

            // Save cart
            await _tempOrderRepository.SetAsync(cart, TimeSpan.FromHours(2));

            return GuestOrderResult.Success(cart);
        }

   
        public async Task<GuestOrderResult> ClearCartAsync(string sessionId)
        {

            await _tempOrderRepository.DeleteAsync(sessionId);
            return GuestOrderResult.Success(new TempOrder { TempOrderId = sessionId, Items = new List<TempOrderItem>() });
        }

        public async Task<GuestOrderResult> GetCartDetailsAsync(string sessionId)
        {

            var cart = await _tempOrderRepository.GetAsync(sessionId);

            if (cart == null || !cart.Items.Any())
                return GuestOrderResult.Failed("Cart is empty");

            // Convert to TempOrderItem for display
            var cartItems = cart.Items.Select(x => new TempOrderItem
            {
                MenuItemId = x.MenuItemId,
                MenuItemName = x.MenuItemName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice
            }).ToList();

            return GuestOrderResult.Success(cartItems, cart.TotalAmount);
        }

        public async Task<GuestOrderResult> CreateTempOrderAsync(string customerName, string? customerPhone, int tableId, int branchId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return GuestOrderResult.Failed("Customer name is required");


            var cart = await _tempOrderRepository.GetAsync(sessionId);

            if (cart == null || !cart.Items.Any())
                return GuestOrderResult.Failed("Cart is empty");

            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            var branch = await _uow.Branches.GetByIdAsync(branchId);

            if (table == null || branch == null)
                return GuestOrderResult.Failed("Table or branch not found");

            // Create temp order from cart
            var tempOrderId = Guid.NewGuid().ToString();
            var tempOrder = new TempOrder
            {
                TempOrderId = tempOrderId,
                BranchId = branchId,
                TableId = tableId,
                CustomerName = customerName.Trim(),
                CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone.Trim(),
                Items = cart.Items.ToList(), // Copy items from cart
                TotalAmount = cart.TotalAmount,
                DiscountAmount = cart.DiscountAmount,
                PayableAmount = cart.PayableAmount,
                RedeemPoints = cart.RedeemPoints,
                PaymentReference = $"TEMP-{tempOrderId}",
                CreatedAt = DateTime.UtcNow
            };

            // Save temp order to Redis
            await _tempOrderRepository.SetAsync(tempOrder);

            return GuestOrderResult.Success(tempOrder);
        }
    }
}