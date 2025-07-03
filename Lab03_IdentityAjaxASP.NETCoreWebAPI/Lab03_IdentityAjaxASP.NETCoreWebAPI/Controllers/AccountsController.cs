using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Account;
using DataAccess.Paginated;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interface;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountsController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            int pageIndex = 1,
            int pageSize = 10,
            string? idSearch = null,
            string? nameSearch = null,
            string? emailSearch = null,
            RoleEnum? roleIdSearch = null)
        {
            try
            {
                PaginatedList<Account> accounts = await _accountRepository.GetAllAccounts(pageIndex, pageSize, idSearch, nameSearch, emailSearch, roleIdSearch);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountDTO account)
        {
            if (account == null)
            {
                return BadRequest("Account data is null.");
            }
            try
            {
                await _accountRepository.CreateAccount(account);
                return CreatedAtAction(nameof(Get), new { id = 0 }, account);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountDTO account)
        {
            if (account == null || id <= 0)
            {
                return BadRequest("Invalid account data or ID.");
            }
            try
            {
                await _accountRepository.UpdateAccount(id, account);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Account with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid account ID.");
            }
            try
            {
                await _accountRepository.DeleteAccount(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Account with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
