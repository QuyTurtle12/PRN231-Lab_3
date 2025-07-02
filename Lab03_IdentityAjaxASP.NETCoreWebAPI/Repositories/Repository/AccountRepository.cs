using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Account;
using DataAccess.Interface;
using DataAccess.Paginated;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IUOW _uow;

        public AccountRepository(IUOW uow)
        {
            _uow = uow;
        }

        public async Task<Account?> Login(string email, string password)
        {
            Account? user = await _uow.GetDAO<Account>()
                .Entities
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return null;

            // Verify password using BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            return isValid ? user : null;
        }

        public async Task<PaginatedList<Account>> GetAllAccounts(
            int pageIndex,
            int pageSize,
            string? idSearch,
            string? nameSearch,
            string? emailSearch,
            RoleEnum? roleSearch)
        {
            var query = _uow.GetDAO<Account>()
                .Entities
                .Include(a => a.Role)
                .Select(a => new Account
                {
                    AccountId = a.AccountId,
                    AccountName = a.AccountName,
                    Email = a.Email,
                    RoleId = a.RoleId,
                    Role = a.Role,
                    Password = null
                });

            // Validate pageIndex and pageSize
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            // Apply filters based on the search parameters
            if (!string.IsNullOrEmpty(idSearch))
            {
                query = query.Where(a => a.AccountId.ToString() == idSearch);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(a => a.AccountName.Contains(nameSearch));
            }

            if (!string.IsNullOrEmpty(emailSearch))
            {
                query = query.Where(a => a.Email.Contains(emailSearch));
            }

            if (roleSearch.HasValue)
            {
                query = query.Where(a => a.RoleId == (int)roleSearch.Value);
            }

            // Sort the results by AccountName
            query = query.OrderBy(a => a.AccountName);

            // Get the paginated list
            PaginatedList<Account> result = await _uow.GetDAO<Account>().GetPagging(query, pageIndex, pageSize);

            return result;
        }
        public async Task CreateAccount(CreateAccountDTO account)
        {
            // Add DTO data to Business Object
            Account newAccount = new Account
            {
                AccountName = account.AccountName,
                Email = account.Email,
                Password = account.Password,
                RoleId = account.RoleId
            };

            // Get latest AccountId and increment it by 1
            newAccount.AccountId = await _uow.GetDAO<Account>()
                .Entities
                .OrderByDescending(a => a.AccountId)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync() + 1;

            // Insert the new account into the database
            await _uow.GetDAO<Account>().InsertAsync(newAccount);
            await _uow.SaveAsync();
        }

        public async Task UpdateAccount(int id, UpdateAccountDTO account)
        {
            // Get the existing account by ID
            Account? existingAccount = await _uow.GetDAO<Account>()
                .Entities
                .FirstOrDefaultAsync(a => a.AccountId == id);

            // Validate if the account exists
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }

            // Assign the updated values from the DTO to the existing account
            existingAccount.AccountName = account.AccountName;
            existingAccount.Email = account.Email;
            existingAccount.Password = account.Password;

            // Save the changes to the database
            await _uow.GetDAO<Account>().UpdateAsync(existingAccount);
            await _uow.SaveAsync();
        }

        public async Task DeleteAccount(int id)
        {
            // Get the existing account by ID
            Account? existingAccount = await _uow.GetDAO<Account>()
                .Entities
                .FirstOrDefaultAsync(a => a.AccountId == id);

            // Validate if the account exists
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }

            // Remove the existing account from the database
            await _uow.GetDAO<Account>().DeleteAsync(existingAccount);
            await _uow.SaveAsync();
        }
    }
}
