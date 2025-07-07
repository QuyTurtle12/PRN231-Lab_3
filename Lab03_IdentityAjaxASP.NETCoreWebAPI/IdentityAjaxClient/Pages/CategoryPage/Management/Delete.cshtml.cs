using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessObjects;
using System.Linq;
using IdentityAjaxClient.Model;

namespace IdentityAjaxClient.Pages.CategoryPage.Management
{
    public class DeleteModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public bool HasRelatedOrchids { get; set; }
        public int RelatedOrchidsCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Check if user is authorized (Staff)
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You are not authorized to delete categories.";
                return RedirectToPage("/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Fetch category details
                var categoryResponse = await _httpClient.GetAsync($"categories?idSearch={id}");
                if (!categoryResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var categoryPaginatedList = await categoryResponse.Content.ReadFromJsonAsync<PaginationDTO<Category>>();
                var category = categoryPaginatedList?.Items.FirstOrDefault();

                if (category == null)
                {
                    return NotFound();
                }

                Category = category;

                // Check if there are orchids using this category
                var orchidResponse = await _httpClient.GetAsync($"orchids?categoryIdSearch={id}");
                if (orchidResponse.IsSuccessStatusCode)
                {
                    var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();
                    RelatedOrchidsCount = orchidPaginatedList?.TotalCount ?? 0;
                    HasRelatedOrchids = RelatedOrchidsCount > 0;
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category with ID: {CategoryId}", id);
                TempData["ErrorMessage"] = "Error loading category. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // Check if user is authorized (Staff)
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You are not authorized to delete categories.";
                return RedirectToPage("/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Check if there are orchids using this category
                var orchidResponse = await _httpClient.GetAsync($"orchids?categoryIdSearch={id}");
                if (orchidResponse.IsSuccessStatusCode)
                {
                    var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();
                    if ((orchidPaginatedList?.TotalCount ?? 0) > 0)
                    {
                        TempData["ErrorMessage"] = "Cannot delete this category because it is being used by one or more orchids.";
                        return RedirectToPage("./Index");
                    }
                }

                // Send DELETE request to remove the category
                var response = await _httpClient.DeleteAsync($"categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API returned error: {StatusCode}, Content: {Content}",
                        response.StatusCode, errorContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return NotFound();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        TempData["ErrorMessage"] = "Cannot delete this category because it is being used by one or more orchids.";
                        return RedirectToPage("./Index");
                    }

                    TempData["ErrorMessage"] = "Error deleting category. Please try again.";
                    return RedirectToPage("./Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the category. Please try again.";
                return RedirectToPage("./Index");
            }
        }
    }
}