using DataAccess.DTO.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace IdentityAjaxClient.Pages.AuthPage
{
    public class LoginModel : PageModel
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
                            Response.Cookies.Append("JWTToken", result.Token, cookieOptions);
                        }

                        HttpContext.Session.SetString("JWTToken", result.Token);
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

    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}