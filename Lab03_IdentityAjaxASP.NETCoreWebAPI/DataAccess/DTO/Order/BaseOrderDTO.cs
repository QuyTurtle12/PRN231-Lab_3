using DataAccess.Constant.Enum;

namespace DataAccess.DTO.Order
{
    public class BaseOrderDTO
    {
        public int? AccountId { get; set; }
        public string? OrderStatus { get; set; } = OrderStatusEnum.Pending.ToString();
    }
}
