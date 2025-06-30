using System.ComponentModel.DataAnnotations;

namespace DataAccess.Constant.Enum
{
    public enum OrderStatusEnum
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Processing")]
        Processing,

        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "Cancelled")]
        Cancelled
    }
}
