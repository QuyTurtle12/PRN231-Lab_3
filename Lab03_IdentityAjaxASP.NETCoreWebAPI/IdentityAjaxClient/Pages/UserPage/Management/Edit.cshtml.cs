using System.Security.Claims;
using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Account;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.UserPage.Management
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public UpdateAccountDTO Account { get; set; } = default!;

        // Store the role name for display purposes only
        public string? RoleName { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get current user info
                var userIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                int currentUserId = 0;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                bool isAdmin = !string.IsNullOrEmpty(userRole) &&
                    userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

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

                // Map to UpdateAccountDTO - without role
                Account = new UpdateAccountDTO
                {
                    AccountName = account.AccountName,
                    Email = account.Email
                };

                // Store role name for display only
                RoleName = account.Role?.RoleName;

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error loading user: {ex.Message}");
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Get current user info
                var userIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                int currentUserId = 0;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                bool isAdmin = !string.IsNullOrEmpty(userRole) &&
                    userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                // Check authorization - Admin can edit all, users can only edit their own
                if (!isAdmin && currentUserId != id)
                {
                    return RedirectToPage("/Index");
                }

                // Send update to API
                var response = await _httpClient.PutAsJsonAsync($"accounts/{id}", Account);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToPage("./Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to update user: {errorContent}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return Page();
            }
        }
    }
}