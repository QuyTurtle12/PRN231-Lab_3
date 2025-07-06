namespace DataAccess.DTO.Order
{
    public class CreateOrderDTO : BaseOrderDTO
    {
        public List<CartItem> OrderItems { get; set; } = new List<CartItem>();
        public string customerEmail { get; set; } = string.Empty;
    }

    public class CartItem : CreateOrderDTO
    {
        public int OrchidId { get; set; }
        public int Quantity { get; set; } = 1;
        public int UserId { get; set; }
    }
}
