using DataAccess.DTO.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.AuthPage
{
    public class RegisterModel : BasePageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public RegisterDTO RegisterDTO { get; set; } = new();

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // The password will be hashed on the API side
                var response = await _httpClient.PostAsJsonAsync("auths/register", RegisterDTO);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/AuthPage/Login");
                }

                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error occurred during registration.");
                return Page();
            }
        }
    }
}
