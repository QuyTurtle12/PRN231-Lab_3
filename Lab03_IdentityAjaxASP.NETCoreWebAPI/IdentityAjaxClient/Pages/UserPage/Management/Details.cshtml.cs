using System.Security.Claims;
using BusinessObjects;
using DataAccess.Constant.Enum;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.UserPage.Management
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        private const string ADMIN_ROLE = "Admin";

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get user info from session
                var userIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                int currentUserId = 0;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                bool isAdmin = !string.IsNullOrEmpty(userRole) &&
                    userRole.Equals(ADMIN_ROLE, StringComparison.OrdinalIgnoreCase);

                // Check authorization - Admin can edit all, users can only edit their own
                if (!isAdmin && currentUserId != id)
                {
                    return RedirectToPage("/Index");
                }

                // Fetch account by id
                var response = await _httpClient.GetAsync($"accounts?idSearch={id}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var accountPaginatedList = await response.Content.ReadFromJsonAsync<PaginationDTO<Account>>();

                Account? account = accountPaginatedList?.Items.FirstOrDefault();

                if (account == null)
                {
                    return NotFound();
                }

                Account = account;

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error loading user: {ex.Message}");
                return RedirectToPage("./Index");
            }
        }
    }
}