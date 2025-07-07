using BusinessObjects;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class DetailsModel : BasePageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public Orchid Orchid { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "No orchid ID provided.";
                return RedirectToPage("./Index");
            }

            try
            {
                // Get current user role from session
                var userRole = HttpContext.Session.GetString("UserRole");

                bool isStaff = !string.IsNullOrEmpty(userRole) &&
                    userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase);

                // Check authorization
                if (!isStaff)
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this page.";
                    return RedirectToPage("/Index");
                }

                // Fetch orchid details
                var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={id}");
                if (!orchidResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to retrieve orchid details.";
                    return RedirectToPage("./Index");
                }

                var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();

                Orchid? orchid = orchidPaginatedList?.Items.FirstOrDefault();

                if (orchid == null)
                {
                    TempData["ErrorMessage"] = "Orchid not found.";
                    return RedirectToPage("./Index");
                }

                Orchid = orchid;

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading orchid: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}
