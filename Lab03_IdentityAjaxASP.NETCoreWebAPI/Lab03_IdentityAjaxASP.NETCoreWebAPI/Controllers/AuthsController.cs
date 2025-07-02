using BusinessObjects;
using DataAccess.DTO.Account;
using DataAccess.DTO.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Auth;
using Repositories.Interface;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IAccountRepository _accountRepository;

        public AuthsController(JwtService jwtService, IAccountRepository accountRepository)
        {
            _jwtService = jwtService;
            _accountRepository = accountRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var user = await _accountRepository.Login(loginDto.Email, loginDto.Password);

            if (user == null)
                return Unauthorized();

            var token = _jwtService.GenerateToken(user);

            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Hash the password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                CreateAccountDTO account = new CreateAccountDTO
                {
                    Email = model.Email,
                    Password = hashedPassword, // Store hashed password
                    AccountName = model.AccountName ?? model.Email,
                    RoleId = model.RoleId
                };

                await _accountRepository.CreateAccount(account);

                return Ok(new { message = "Registration successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }
    }
}
