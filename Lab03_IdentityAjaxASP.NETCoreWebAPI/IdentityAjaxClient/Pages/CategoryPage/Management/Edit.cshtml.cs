using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessObjects;
using DataAccess.DTO.Category;
using IdentityAjaxClient.Model;

namespace IdentityAjaxClient.Pages.CategoryPage.Management
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        [BindProperty]
        public UpdateCategoryDTO CategoryInput { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Check if user is authorized
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Fetch category
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

                CategoryInput = new UpdateCategoryDTO
                {
                    Name = category.CategoryName ?? string.Empty
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching category with ID: {id}");
                TempData["ErrorMessage"] = "Error loading category. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // Check if user is authorized
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (id == null || CategoryInput == null)
            {
                return BadRequest("Invalid category ID or input data.");
            }

            try
            {

                // Send PUT request to update the category
                var response = await _httpClient.PutAsJsonAsync($"categories/{id}", CategoryInput);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category updated successfully!";
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

                    ModelState.AddModelError(string.Empty, "Error updating category. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category with ID: {id}");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again.");
                return Page();
            }
        }
    }
}