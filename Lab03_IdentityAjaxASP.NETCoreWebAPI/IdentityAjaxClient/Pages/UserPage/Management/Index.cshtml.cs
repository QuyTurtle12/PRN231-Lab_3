using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessObjects;
using IdentityAjaxClient.Model;
using DataAccess.Constant.Enum;

namespace IdentityAjaxClient.Pages.UserPage.Management
{
    public class IndexModel : BasePageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public PaginationDTO<Account> Account { get; set; } = default!;
        public SelectList? RoleList { get; set; }

        // Search and filter properties
        [BindProperty(SupportsGet = true)]
        public string? NameSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EmailSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RoleIdSearch { get; set; }

        public async Task<IActionResult> OnGetAsync(
            int pageIndex = 1,
            int pageSize = 10)
        {
            try
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
                    "Name",
                    RoleIdSearch
                );

                // Build query string for accounts
                var query = $"accounts?pageIndex={pageIndex}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(NameSearch))
                {
                    query += $"&nameSearch={Uri.EscapeDataString(NameSearch)}";
                }

                if (!string.IsNullOrWhiteSpace(EmailSearch))
                {
                    query += $"&emailSearch={Uri.EscapeDataString(EmailSearch)}";
                }

                if (RoleIdSearch.HasValue)
                {
                    query += $"&roleIdSearch={RoleIdSearch.Value}";
                }

                // Fetch accounts with pagination and filtering
                var response = await _httpClient.GetFromJsonAsync<PaginationDTO<Account>>(query);

                if (response != null)
                {
                    Account = response;
                }
                else
                {
                    Account = new PaginationDTO<Account>
                    {
                        Items = new List<Account>(),
                        PageNumber = 1,
                        PageSize = 10,
                        TotalCount = 0,
                        TotalPages = 0
                    };
                    ModelState.AddModelError(string.Empty, "No accounts found.");
                }
            }
            catch (Exception ex)
            {
                Account = new PaginationDTO<Account>
                {
                    Items = new List<Account>(),
                    PageNumber = 1,
                    PageSize = 10,
                    TotalCount = 0,
                    TotalPages = 0
                };
                ModelState.AddModelError(string.Empty, "Error loading accounts: " + ex.Message);
            }

            return Page();
        }
    }
}