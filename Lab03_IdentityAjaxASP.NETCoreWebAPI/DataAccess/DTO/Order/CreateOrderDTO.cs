namespace DataAccess.DTO.Order
{
    public class CreateOrderDTO : BaseOrderDTO
    {
        public int AccountId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CartItem> OrderItems { get; set; } = new List<CartItem>();
    }

    public class CartItem : CreateOrderDTO
    {
        public int OrchidId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
    }
}
