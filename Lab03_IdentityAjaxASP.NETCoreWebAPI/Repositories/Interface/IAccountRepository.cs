using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Account;
using DataAccess.Paginated;

namespace Repositories.Interface
{
    public interface IAccountRepository
    {
        Task<Account?> Login(string email, string password);
        Task<PaginatedList<Account>> GetAllAccounts(
            int pageIndex,
            int pageSize,
            string? idSearch,
            string? nameSearch,
            string? emailSearch,
            RoleEnum? roleSearch);
        Task CreateAccount(CreateAccountDTO account);
        Task UpdateAccount(int id, UpdateAccountDTO account);
        Task DeleteAccount(int id);
    }
}
