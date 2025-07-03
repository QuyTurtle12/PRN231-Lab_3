using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Account;

namespace IdentityAjaxClient.Pages.UserPage.Management
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public SelectList? RoleList { get; set; }

        [BindProperty]
        public CreateAccountDTO Account { get; set; } = default!;

        public IActionResult OnGet()
        {
            // Get current user role from session
            var userRole = HttpContext.Session.GetString("UserRole");

            bool isAdmin = !string.IsNullOrEmpty(userRole) &&
                userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // Check authorization
            if (!isAdmin)
            {
                return RedirectToPage("/Index");
            }

            // Create SelectList from RoleEnum
            RoleList = new SelectList(
                Enum.GetValues<RoleEnum>()
                    .Select(r => new { Id = (int)r, Name = r.ToString() }),
                "Id",
                "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get current user role from session
            var userRole = HttpContext.Session.GetString("UserRole");

            bool isAdmin = !string.IsNullOrEmpty(userRole) &&
                userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // Check authorization
            if (!isAdmin)
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                // Recreate the role list if validation fails
                RoleList = new SelectList(
                    Enum.GetValues<RoleEnum>()
                        .Select(r => new { Id = (int)r, Name = r.ToString() }),
                    "Id",
                    "Name",
                    Account.RoleId);

                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("accounts", Account);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToPage("./Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to create user: {errorContent}");

                // Recreate role list after error
                RoleList = new SelectList(
                    Enum.GetValues<RoleEnum>()
                        .Select(r => new { Id = (int)r, Name = r.ToString() }),
                    "Id",
                    "Name",
                    Account.RoleId);

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");

                // Recreate role list after exception
                RoleList = new SelectList(
                    Enum.GetValues<RoleEnum>()
                        .Select(r => new { Id = (int)r, Name = r.ToString() }),
                    "Id",
                    "Name",
                    Account.RoleId);

                return Page();
            }
        }
    }
}