namespace CoffeeShop.Application.DTO
{
    public class CreateOrderItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}