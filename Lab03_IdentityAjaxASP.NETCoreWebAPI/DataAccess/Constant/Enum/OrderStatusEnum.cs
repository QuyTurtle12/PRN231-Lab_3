using System.ComponentModel.DataAnnotations;

namespace DataAccess.Constant.Enum
{
    public enum OrderStatusEnum
    {
        Pending = 1,
        Processing = 2,
        Shipping = 3,
        Delivered = 4,
        Cancelled = 5
    }
}
