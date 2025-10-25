using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Entities
{
    public class TempOrder
    {
        public string TempOrderId { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public int? TableId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public List<TempOrderItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public int RedeemPoints { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TempOrderItem
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
