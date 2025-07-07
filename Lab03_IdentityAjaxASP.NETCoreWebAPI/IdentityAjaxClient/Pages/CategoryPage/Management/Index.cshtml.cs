using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessObjects;
using IdentityAjaxClient.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityAjaxClient.Pages.CategoryPage.Management
{
    public class IndexModel : BasePageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public PaginationDTO<Category> Categories { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Check if the user is authorized
                var userRole = HttpContext.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(userRole) || (userRole != "Staff"))
                {
                    return RedirectToPage("/Index");
                }

                // Fetch categories from API
                var response = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");
                if (response != null)
                {
                    Categories = response;
                }
                return Page();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}