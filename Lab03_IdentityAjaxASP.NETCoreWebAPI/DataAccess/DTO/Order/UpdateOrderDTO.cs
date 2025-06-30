using DataAccess.Constant.Enum;

namespace DataAccess.DTO.Order
{
    public class UpdateOrderDTO
    {
        public string? OrderStatus { get; set; } = OrderStatusEnum.Pending.ToString();
        public List<CartItem> OrderItems { get; set; } = new List<CartItem>();
    }
}
