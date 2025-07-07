using DataAccess.DTO.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace IdentityAjaxClient.Pages.AuthPage
{
    public class LoginModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public LoginDTO LoginDTO { get; set; } = new();

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _configuration = configuration;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("auths/login", LoginDTO);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    if (result?.Token != null)
                    {
                        // Store token in session
                        if (LoginDTO.RememberMe)
                        {
                            // Set a persistent cookie for the token
                            var cookieOptions = new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(30),
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict
                            };
                            // Store JWT, UserId and UserRole in cookies
                            Response.Cookies.Append("JWTToken", result.Token, cookieOptions);
                            Response.Cookies.Append("UserId", result.AccountId.ToString(), cookieOptions);
                            Response.Cookies.Append("UserRole", result.RoleName, cookieOptions);
                            Response.Cookies.Append("UserName", result.RoleName, cookieOptions);
                        }

                        // Store token and user info in session (for current session)
                        HttpContext.Session.SetString("JWTToken", result.Token);
                        HttpContext.Session.SetString("UserId", result.AccountId.ToString());
                        HttpContext.Session.SetString("UserRole", result.RoleName);
                        HttpContext.Session.SetString("UserName", result.AccountName);
                        return RedirectToPage("/Index");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error occurred during login.");
                return Page();
            }
        }
    }
}