using DataAccess.Constant.Enum;

namespace DataAccess.DTO.Account
{
    public class CreateAccountDTO : BaseAccountDTO
    {
        public int RoleId { get; set; } = (int)RoleEnum.Customer;
    }
}
